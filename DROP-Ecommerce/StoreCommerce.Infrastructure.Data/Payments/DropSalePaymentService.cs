using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Payments;

public sealed class DropSalePaymentService(StoreCommerceDbContext db, IIdentityOperationScope identity,
    DropSalePaymentWriteScope writes, IDropSaleGateway gateway, TimeProvider time)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<DropBuyerProfile> ProfileAsync(long tenant, long customerId, CancellationToken ct)
    {
        var customer = await db.Customers.IgnoreQueryFilters().AsNoTracking().Include(Customer => Customer.State)
            .SingleOrDefaultAsync(Customer => Customer.Id == customerId && Customer.EnterpriseId == tenant && Customer.IsActive && !Customer.IsDeleted, ct)
            ?? throw new SecurityFailure(404, "DropPayment.CustomerUnavailable", "Comprador indisponível.");

        return new DropBuyerProfile(customer.Id, tenant, customer.FullName, customer.Email.Value,
            customer.AddressLine, customer.City, customer.State?.Description ?? "", customer.ZipCode);
    }

    public async Task<DropSalePaymentReceipt> StartAsync(long tenant, StartDropSalePayment request, CancellationToken ct)
    {
        if (request.OperationId == Guid.Empty || request.DropOrderId < 1 || request.CustomerId < 1 || request.Amount <= 0 ||
            request.Amount > 9999999999999999.99m || decimal.Round(request.Amount, 2) != request.Amount || request.Currency != "BRL")
            throw new SecurityFailure(400, "DropPayment.InvalidIntent", "Intenção de pagamento inválida.");

        var existing = await db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Intent => Intent.EnterpriseId == tenant && Intent.OperationId == request.OperationId, ct);

        if (existing is not null)
        {
            if (existing.DropOrderId != request.DropOrderId || existing.CustomerId != request.CustomerId ||
                existing.Amount != request.Amount || existing.ExpiresAt != request.ExpiresAt || existing.Currency != request.Currency)
                throw new SecurityFailure(409, "DropPayment.IdempotencyConflict", "A operação já identifica outra cobrança.");

            return await ReconcileAsync(tenant, request.OperationId, ct);
        }

        if (request.ExpiresAt.Kind != DateTimeKind.Utc || request.ExpiresAt <= Now || request.ExpiresAt > Now.AddMinutes(15))
            throw new SecurityFailure(409, "DropPayment.InvalidDeadline", "O prazo da cobrança deve respeitar a sessão de compra.");

        var context = await gateway.ContextAsync(tenant, ct);
        await ProfileAsync(tenant, request.CustomerId, ct);

        using (identity.Begin())
        using (writes.Begin(tenant))
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM cliente WHERE id = {request.CustomerId} AND enterprise_id = {tenant} FOR UPDATE", ct);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
            existing = await db.Set<DropSalePaymentIntent>().IgnoreQueryFilters()
                .SingleOrDefaultAsync(Intent => Intent.EnterpriseId == tenant && Intent.OperationId == request.OperationId, ct);

            if (existing is not null && (existing.DropOrderId != request.DropOrderId || existing.CustomerId != request.CustomerId ||
                existing.Amount != request.Amount || existing.ExpiresAt != request.ExpiresAt || existing.Currency != request.Currency))
                throw new SecurityFailure(409, "DropPayment.IdempotencyConflict", "A operação já identifica outra cobrança.");

            if (existing is null)
            {
                await ProfileAsync(tenant, request.CustomerId, ct);
                db.Add(DropSalePaymentIntent.Prepare(tenant, request.OperationId, request.DropOrderId, request.CustomerId,
                    request.Amount, request.ExpiresAt, context.Provider, context.Environment, context.ExternalAccountId,
                    context.ProviderConnectionId, Now));
                await db.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        db.ChangeTracker.Clear();

        return await ReconcileAsync(tenant, request.OperationId, ct);
    }

    public async Task<DropSalePaymentReceipt> ReconcileAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        using var operation = identity.Begin();
        using var payment = writes.Begin(tenant);
        DropSalePaymentIntent intent;
        bool create;

        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            await LockAsync(tenant, ct);
            intent = await ReadAsync(tenant, operationId, ct);
            create = !intent.SubmittedAt.HasValue;

            if (create && Now >= intent.ExpiresAt)
            {
                intent.Observe("not_submitted", null);
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return Receipt(intent);
            }

            if (create)
            {
                intent.Submit(Now);
                await db.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        if (intent.Status == "not_submitted")
            return Receipt(intent);

        var profile = create ? await ProfileAsync(tenant, intent.CustomerId, ct) : null;
        var request = new PaymentRequest(tenant, intent.DropOrderId, intent.CustomerId, profile?.Name ?? "Comprador DROP",
            profile?.Email ?? "", intent.Amount, intent.Currency, 1, intent.ExpiresAt, intent.OperationId.ToString("N"),
            new PaymentProviderContext(intent.Provider, intent.Environment, intent.ExternalAccountId, intent.ProviderConnectionId), intent.ExternalReference, "drop");
        PaymentSession? checkout = null;
        DropSaleGatewayResult? result = null;

        if (intent.PreferenceId is null)
        {
            try
            {
                checkout = await gateway.CreateAsync(request, !create, ct);
            }
            catch (PaymentNotSubmittedException) when (create)
            {
                result = new DropSaleGatewayResult("not_submitted", null);
            }
        }

        if (intent.PreferenceId is not null || checkout is not null)
            result = await gateway.ReadAsync(request, ct);
        db.ChangeTracker.Clear();
        await using var persist = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        intent = await ReadAsync(tenant, operationId, ct);

        if (checkout is not null)
            intent.Checkout(checkout.PreferenceId ?? checkout.Reference, checkout.CheckoutUrl);

        if (result is not null)
            intent.Observe(result.Status, result.PaymentId);

        intent.CheckAfter(Now.AddMinutes(1));
        await db.SaveChangesAsync(ct);
        await persist.CommitAsync(ct);

        return Receipt(intent);
    }

    public async Task AcknowledgeAsync(long tenant, Guid operationId, long revision, bool fulfillmentClosed, bool requiresReview, CancellationToken ct)
    {
        using var operation = identity.Begin();
        using var payment = writes.Begin(tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var intent = await ReadAsync(tenant, operationId, ct);
        intent.Acknowledge(revision);

        if (requiresReview)
            intent.RequireFulfillmentReview();

        if (fulfillmentClosed)
            intent.CloseFulfillment();

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public Task<List<DropSalePaymentIntent>> DueAsync(CancellationToken ct)
    {
        return db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
            .Where(Intent => Intent.Status == "pending" && Intent.NextCheckAt <= Now)
            .OrderBy(Intent => Intent.NextCheckAt).ThenBy(Intent => Intent.Id).Take(50).ToListAsync(ct);
    }

    public async Task PostponeAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        using var operation = identity.Begin();
        using var payment = writes.Begin(tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, ct);
        var intent = await ReadAsync(tenant, operationId, ct);
        intent.CheckAfter(Now.AddMinutes(1));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public static DropSalePaymentReceipt Receipt(DropSalePaymentIntent intent)
    {
        return new DropSalePaymentReceipt(intent.OperationId, intent.EnterpriseId, intent.DropOrderId, intent.CustomerId,
            intent.Amount, intent.Currency, intent.Status, intent.CheckoutUrl, intent.PaymentId, intent.Revision);
    }

    private async Task<DropSalePaymentIntent> ReadAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        return await db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().SingleOrDefaultAsync(Intent => Intent.EnterpriseId == tenant && Intent.OperationId == operationId, ct)
            ?? throw new SecurityFailure(404, "DropPayment.IntentNotFound", "Intenção de pagamento não encontrada.");
    }

    private Task LockAsync(long tenant, CancellationToken ct)
    {
        return db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
    }
}
