using System.Security.Cryptography;
using System.Text;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Contracts;

public sealed class JoinDropQueueRequest
{
    public string CPF { get; init; } = string.Empty;
}

public sealed record ReserveDropVariant(long VariantId, int Quantity);
public sealed record ReserveDropPurchase(long ParticipationId, long SessionId, string IdempotencyKey, IReadOnlyList<ReserveDropVariant> Lines);

public sealed class DropQueueService(DropCommerceDbContext db, DropCommercialWriteScope writes,
    IStoreCustomerReader customers, ITenantProvider tenants, IHttpContextAccessor http,
    IOptions<DropQueueOptions> options, TimeProvider time, StoreIntegrationScope integration)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;
    private long Customer => http.HttpContext?.User.FindFirst(SecurityNames.Actor)?.Value == "customer"
        ? SecurityNames.Id(http.HttpContext.User, SecurityNames.Customer)
        : throw new SecurityFailure(403, "DropPurchase.CustomerRequired", "Autenticação de comprador necessária.");

    public async Task<object> JoinAsync(long eventId, JoinDropQueueRequest request, CancellationToken ct)
    {
        var tenant = tenants.GetEnterpriseId();
        var customer = Customer;
        using var storeOperation = integration.Begin(tenant);
        var hash = PurchaseIdentity(options.Value.PurchaseIdentityKey, tenant, eventId, request.CPF);
        var rights = await db.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop || await customers.GetByIdAsync(customer, tenant, ct) is not { IsActive: true })
            throw Failure("DropPurchase.Unavailable", "O módulo ou o comprador não está disponível.");

        using (writes.Begin("sales", tenant))
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.LockCommercialTenantAsync(tenant, ct);
            var drop = await db.DropEvents.SingleOrDefaultAsync(Event => Event.Id == eventId, ct)
                ?? throw new SecurityFailure(404, "Drop.NotFound", "Evento não encontrado.");
            var registered = await db.DropRegistrations.AnyAsync(Registration => Registration.DropEventId == eventId &&
                Registration.CustomerId == customer && Registration.IsEligible, ct);

            if (!DropCommerceDbContext.IsPublished(drop) || Now < drop.QueueOpensAt || Now >= drop.DropEndsAt ||
                (!drop.IsPublic || drop.RequiresRegistration) && !registered || !await HasProductsAsync(tenant, eventId, ct))
                throw Failure("Drop.QueueClosed", "O evento não admite novas entradas.");

            var entry = await db.QueueEntries.IgnoreQueryFilters().SingleOrDefaultAsync(Entry =>
                Entry.DropEventId == eventId && Entry.CustomerId == customer, ct);
            var position = await TailAsync(eventId, ct) + 1;

            if (entry is null)
            {
                var agent = http.HttpContext?.Request.Headers.UserAgent.ToString();
                agent = string.IsNullOrWhiteSpace(agent) ? "unknown" : agent[..Math.Min(500, agent.Length)];
                entry = QueueEntry.Create(eventId, customer, OpaqueToken.Hash(OpaqueToken.Create()), position, 1,
                    "not-collected", http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0", agent,
                    Now, null, null, null);
                db.Add(entry);
            }
            else if (entry.QueueEntryStatusId is 5 or 6)
            {
                entry.Requeue(position, Now);
            }

            if (entry.PurchaseIdentityHash is not null && entry.PurchaseIdentityHash != hash)
                throw Failure("DropPurchase.IdentityConflict", "A identidade de compra não pode mudar dentro do evento.");

            entry.BindPurchaseIdentity(hash);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        db.ChangeTracker.Clear();
        await TickAsync(tenant, eventId, ct);

        return await StatusAsync(eventId, ct);
    }

    public async Task<object> StatusAsync(long eventId, CancellationToken ct)
    {
        var customer = Customer;
        var tenant = tenants.GetEnterpriseId();
        var entry = await db.QueueEntries.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(Entry =>
            Entry.DropEventId == eventId && Entry.CustomerId == customer && Entry.DropEvent.EnterpriseId == tenant, ct)
            ?? throw new SecurityFailure(404, "Drop.QueueNotFound", "Entrada não encontrada.");
        var session = await db.QueueSessions.IgnoreQueryFilters().AsNoTracking().Where(Session => Session.QueueEntryId == entry.Id &&
            Session.CustomerId == customer && Session.QueueSessionStatusId == 1 && Session.ExpiresAt > Now)
            .OrderByDescending(Session => Session.Id).Select(Session => new { Session.Id, Session.IssuedAt, Session.ExpiresAt }).FirstOrDefaultAsync(ct);

        return new { entry.Id, entry.Position, entry.QueueEntryStatusId, session };
    }

    public async Task<DropReservation> ReserveAsync(long eventId, ReserveDropPurchase request, CancellationToken ct)
    {
        var tenant = tenants.GetEnterpriseId();
        var customer = Customer;
        await ExpireAsync(tenant, eventId, ct);
        var rights = await db.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop || string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Length > 100 ||
            request.Lines is null || request.Lines.Count is < 1 or > 200 || request.Lines.Any(Line => Line.Quantity is < 1 or > 1000000) ||
            request.Lines.Select(Line => Line.VariantId).Distinct().Count() != request.Lines.Count)
            throw Failure("DropPurchase.InvalidReservation", "Contrato ou reserva inválida.");

        using var operation = writes.Begin("sales", tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var session = await db.QueueSessions.IgnoreQueryFilters().Include(Session => Session.QueueEntry)
            .ThenInclude(Entry => Entry.DropEvent).SingleOrDefaultAsync(Session => Session.Id == request.SessionId && Session.CustomerId == customer &&
                Session.QueueEntry.DropEventId == eventId && Session.QueueEntry.DropEvent.EnterpriseId == tenant, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.SessionNotFound", "Sessão de compra não encontrada.");

        if (session.QueueSessionStatusId != 1 || Now >= session.ExpiresAt || session.QueueEntry.PurchaseIdentityHash is null ||
            Now >= session.QueueEntry.DropEvent.DropEndsAt)
            throw Failure("DropPurchase.SessionExpired", "O prazo de compra terminou.");

        var existing = await db.DropReservations.IgnoreQueryFilters().Include(Reservation => Reservation.Lines)
            .SingleOrDefaultAsync(Reservation => Reservation.QueueSessionId == session.Id && Reservation.CustomerId == customer &&
                Reservation.IdempotencyKey == request.IdempotencyKey, ct);

        if (existing is not null)
        {
            if (existing.DropProductId != request.ParticipationId || !existing.Lines.OrderBy(Line => Line.VariantId)
                .Select(Line => (Line.VariantId, Line.Quantity)).SequenceEqual(request.Lines.OrderBy(Line => Line.VariantId).Select(Line => (Line.VariantId, Line.Quantity))))
                throw Failure("DropPurchase.IdempotencyConflict", "A chave já identifica outra reserva.");

            return existing;
        }

        var participation = await db.DropProducts.IgnoreQueryFilters().Include(Product => Product.CatalogProduct).ThenInclude(Product => Product!.Variants)
            .SingleOrDefaultAsync(Product => Product.Id == request.ParticipationId && Product.DropEventId == eventId &&
                Product.DropEvent.EnterpriseId == tenant && !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.ProductUnavailable", "Produto indisponível.");
        var product = participation.CatalogProduct;

        if (product is null || product.IsDeleted || product.IsContractBlocked || product.IsReturning || product.IsReturned ||
            product.StoreProductId.HasValue && !product.SourceExportId.HasValue)
            throw Failure("DropPurchase.ProductUnavailable", "O produto não está disponível para reservas.");

        var variants = request.Lines.Select(Line => (Line, Variant: product.Variants.SingleOrDefault(Variant => Variant.Id == Line.VariantId))).ToList();

        if (variants.Any(Item => Item.Variant is null || Item.Line.Quantity > Item.Variant.QuantityOnHand - Item.Variant.QuantityReserved))
            throw Failure("DropPurchase.InsufficientStock", "Há variações indisponíveis ou sem saldo suficiente.");

        var quantity = request.Lines.Sum(Line => (long)Line.Quantity);
        var used = await db.DropReservations.IgnoreQueryFilters().Where(Reservation => Reservation.DropEventId == eventId &&
            Reservation.DropProduct.CatalogProductId == product.Id && Reservation.PurchaseIdentityHash == session.QueueEntry.PurchaseIdentityHash &&
            (Reservation.DropReservationStatusId == 2 || Reservation.DropReservationStatusId == 1 && Reservation.ExpiresAt > Now))
            .SumAsync(Reservation => (long)Reservation.Quantity, ct);

        var limit = await db.DropProducts.IgnoreQueryFilters().Where(Product => Product.DropEventId == eventId &&
            Product.CatalogProductId == product.Id && Product.DropEvent.EnterpriseId == tenant && Product.IsActive && !Product.IsDeleted)
            .MinAsync(Product => Product.MaxPerCustomer, ct);

        if (quantity + used > limit || quantity > int.MaxValue)
            throw Failure("DropPurchase.PurchaseLimit", "A soma das variações e das reservas deste CPF atingiu o limite do produto no evento.");

        var total = variants.Sum(Item => Item.Variant!.Price * Item.Line.Quantity);

        if (total <= 0 || total > 9999999999999999.99m)
            throw Failure("DropPurchase.InvalidTotal", "O total da reserva excede a capacidade de cobrança.");

        var reservation = DropReservation.Create(eventId, participation.Id, customer, session.QueueEntryId, 1, (int)quantity,
            decimal.Round(total / quantity, 2), total, OpaqueToken.Hash(OpaqueToken.Create()), Now, session.ExpiresAt, null, null);
        reservation.BindSession(session.Id, session.QueueEntry.PurchaseIdentityHash);
        reservation.SetIdempotencyKey(request.IdempotencyKey);

        foreach (var item in variants)
        {
            item.Variant!.Reserve(item.Line.Quantity);
            reservation.Lines.Add(DropReservationLine.Capture(reservation, item.Variant, item.Line.Quantity));
        }

        db.Add(reservation);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return reservation;
    }

    public async Task TickAsync(long tenant, long eventId, CancellationToken ct)
    {
        using var storeOperation = integration.Begin(tenant);
        await ExpireAsync(tenant, eventId, ct);
        db.ChangeTracker.Clear();
        var rights = await db.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop)
            return;

        using var operation = writes.Begin("sales", tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var drop = await db.DropEvents.IgnoreQueryFilters().SingleAsync(Event => Event.Id == eventId && Event.EnterpriseId == tenant, ct);

        if (!DropCommerceDbContext.IsPublished(drop) || Now < drop.DropStartsAt || Now >= drop.DropEndsAt || !await HasProductsAsync(tenant, eventId, ct))
            return;

        var expired = await db.QueueEntries.IgnoreQueryFilters().Where(Entry => Entry.DropEventId == eventId && Entry.QueueEntryStatusId == 5 && Entry.PurchaseIdentityHash != null)
            .OrderBy(Entry => Entry.ExpiredAt).ThenBy(Entry => Entry.Id).ToListAsync(ct);
        var position = await TailAsync(eventId, ct);

        foreach (var entry in expired)
            entry.Requeue(++position, Now);

        var active = await db.QueueSessions.IgnoreQueryFilters().CountAsync(Session => Session.QueueEntry.DropEventId == eventId &&
            Session.QueueSessionStatusId == 1 && Session.ExpiresAt > Now, ct);
        var slots = Math.Max(0, Math.Clamp(options.Value.ConcurrentBuyersPerEvent, 1, 1000) - active);
        var waiting = await db.QueueEntries.IgnoreQueryFilters().Where(Entry => Entry.DropEventId == eventId && Entry.QueueEntryStatusId == 1 && Entry.PurchaseIdentityHash != null)
            .OrderBy(Entry => Entry.Position).ThenBy(Entry => Entry.Id).Take(slots).ToListAsync(ct);

        foreach (var entry in waiting)
        {
            if (await customers.GetByIdAsync(entry.CustomerId, tenant, ct) is not { IsActive: true })
                continue;

            if ((!drop.IsPublic || drop.RequiresRegistration) && !await db.DropRegistrations.IgnoreQueryFilters().AnyAsync(Registration =>
                Registration.DropEventId == eventId && Registration.CustomerId == entry.CustomerId && Registration.IsEligible && !Registration.IsDeleted, ct))
                continue;

            entry.Admit(Now);
            db.Add(QueueSession.Create(entry.Id, entry.CustomerId, OpaqueToken.Hash(OpaqueToken.Create()), 1, Now, Now.AddMinutes(15), Now));
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task ExpireAsync(long tenant, long eventId, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        using var operation = writes.Begin("expiration", tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var reservations = await db.DropReservations.IgnoreQueryFilters().Include(Reservation => Reservation.Lines).ThenInclude(Line => Line.Variant)
            .Where(Reservation => Reservation.DropEventId == eventId && Reservation.DropEvent.EnterpriseId == tenant &&
                Reservation.DropReservationStatusId == 1 && Reservation.ExpiresAt <= Now).ToListAsync(ct);

        foreach (var reservation in reservations)
        {
            foreach (var line in reservation.Lines)
                line.Variant.Release(line.Quantity);

            reservation.Expire(Now);
        }

        var expiredIds = reservations.Select(Reservation => Reservation.Id).ToList();
        var orders = await db.DropOrders.IgnoreQueryFilters().Where(Order => expiredIds.Contains(Order.DropReservationId) &&
            Order.DropEvent.EnterpriseId == tenant && Order.DropOrderPaymentStatusId == 1).ToListAsync(ct);

        foreach (var order in orders)
            order.ExpireUnpaid();

        var sessions = await db.QueueSessions.IgnoreQueryFilters().Include(Session => Session.QueueEntry)
            .Where(Session => Session.QueueEntry.DropEventId == eventId && Session.QueueEntry.DropEvent.EnterpriseId == tenant &&
                Session.QueueSessionStatusId == 1 && Session.ExpiresAt <= Now).ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.Expire();
            session.QueueEntry.Expire(Now);
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task<int> TailAsync(long eventId, CancellationToken ct)
    {
        return await db.QueueEntries.IgnoreQueryFilters().Where(Entry => Entry.DropEventId == eventId)
            .Select(Entry => (int?)Entry.Position).MaxAsync(ct) ?? 0;
    }

    private Task<bool> HasProductsAsync(long tenant, long eventId, CancellationToken ct)
    {
        return db.DropProducts.IgnoreQueryFilters().AnyAsync(Product => Product.DropEventId == eventId && Product.DropEvent.EnterpriseId == tenant &&
            !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked && Product.CatalogProduct != null &&
            !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked && !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
            (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null) &&
            Product.CatalogProduct.Variants.Any(Variant => Variant.QuantityOnHand > Variant.QuantityReserved), ct);
    }

    public static string PurchaseIdentity(string secret, long tenant, long eventId, string cpf)
    {
        if (secret.Length < 32)
            throw new SecurityFailure(503, "DropPurchase.IdentityNotConfigured", "Configure a chave privada da franquia de compra.");

        var digits = new string((cpf ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

        if (digits.Length != 11 || digits.Distinct().Count() == 1)
            throw new SecurityFailure(400, "DropPurchase.InvalidCpf", "CPF inválido.");

        for (var length = 9; length <= 10; length++)
        {
            var sum = Enumerable.Range(0, length).Sum(Index => (digits[Index] - '0') * (length + 1 - Index));
            var check = 11 - sum % 11;

            if ((check >= 10 ? 0 : check) != digits[length] - '0')
                throw new SecurityFailure(400, "DropPurchase.InvalidCpf", "CPF inválido.");
        }

        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{tenant}:{eventId}:{digits}")));
    }

    private static SecurityFailure Failure(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }
}
