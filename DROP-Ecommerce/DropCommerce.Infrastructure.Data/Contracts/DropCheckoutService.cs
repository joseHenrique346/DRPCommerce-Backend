using DropCommerce.Application.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Contracts;

public sealed class DropCheckoutService(DropCommerceDbContext db, DropCommercialWriteScope writes,
    IStoreDropPayments payments, ITenantProvider tenants, IHttpContextAccessor http, TimeProvider time, DropQueueService queue)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;
    private long Customer => http.HttpContext?.User.FindFirst(SecurityNames.Actor)?.Value == "customer"
        ? SecurityNames.Id(http.HttpContext.User, SecurityNames.Customer)
        : throw new SecurityFailure(403, "DropPurchase.CustomerRequired", "Autenticação de comprador necessária.");

    public async Task<object> CheckoutAsync(long reservationId, CancellationToken ct)
    {
        var tenant = tenants.GetEnterpriseId();
        var customerId = Customer;
        var profile = await payments.ProfileAsync(tenant, customerId, ct);

        if (new[] { profile.AddressLine, profile.City, profile.State, profile.ZipCode }.Any(string.IsNullOrWhiteSpace))
            throw new SecurityFailure(409, "DropPurchase.AddressRequired", "Complete o endereço antes de iniciar a cobrança.");

        long orderId;

        using (writes.Begin("sales", tenant))
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.LockCommercialTenantAsync(tenant, ct);
            var reservation = await db.DropReservations.IgnoreQueryFilters().Include(Reservation => Reservation.Lines)
                .SingleOrDefaultAsync(Reservation => Reservation.Id == reservationId && Reservation.CustomerId == customerId &&
                    Reservation.DropEvent.EnterpriseId == tenant, ct)
                ?? throw new SecurityFailure(404, "DropPurchase.ReservationNotFound", "Reserva não encontrada.");
            var order = await db.DropOrders.IgnoreQueryFilters().SingleOrDefaultAsync(Order => Order.DropReservationId == reservationId &&
                Order.CustomerId == customerId && Order.DropEvent.EnterpriseId == tenant && Order.PaymentOperationId != null, ct);

            if (order is null)
            {
                if (reservation.DropReservationStatusId != 1 || Now >= reservation.ExpiresAt || !reservation.QueueSessionId.HasValue || reservation.Lines.Count == 0)
                    throw new SecurityFailure(409, "DropPurchase.ReservationExpired", "A reserva não está disponível para iniciar cobrança.");

                order = DropOrder.Create(reservation.DropEventId, customerId, reservation.Id, null, 1, 1,
                    reservation.TotalAmount, 0, 0, 0, reservation.TotalAmount,
                    profile.AddressLine, profile.City, profile.State, profile.ZipCode, null);
                order.PreparePayment(profile.Name, profile.Email);
                db.Add(order);
                await db.SaveChangesAsync(ct);

                foreach (var line in reservation.Lines)
                {
                    var item = DropOrderItem.Create(order.Id, reservation.DropProductId, line.ItemName, line.SKU,
                        line.Quantity, line.UnitPrice, line.Quantity * line.UnitPrice);
                    item.BindVariantSnapshot(line.VariantId, line.OptionsJson);
                    db.Add(item);
                }

                await db.SaveChangesAsync(ct);
            }

            orderId = order.Id;
            await tx.CommitAsync(ct);
        }

        db.ChangeTracker.Clear();
        await ReconcileOrderAsync(tenant, orderId, ct);

        return await GetOrderAsync(orderId, ct);
    }

    public async Task ReconcileOrderAsync(long tenant, long orderId, CancellationToken ct)
    {
        var order = await db.DropOrders.IgnoreQueryFilters().AsNoTracking().Include(Order => Order.DropReservation)
            .SingleAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);

        if (!order.PaymentOperationId.HasValue)
            return;

        var receipt = !order.PaymentIntentAccepted && Now < order.DropReservation.ExpiresAt
            ? await payments.StartAsync(tenant, new(order.PaymentOperationId.Value, order.Id, order.CustomerId,
                order.TotalAmount, "BRL", order.DropReservation.ExpiresAt), ct)
            : await payments.GetAsync(tenant, order.PaymentOperationId.Value, ct);

        if (receipt is not null)
            await ApplyReceiptAsync(receipt, ct);
    }

    public async Task ApplyReceiptAsync(DropSalePaymentReceipt receipt, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        var eventId = await db.DropOrders.IgnoreQueryFilters().Where(Order => Order.Id == receipt.DropOrderId &&
            Order.DropEvent.EnterpriseId == receipt.EnterpriseId).Select(Order => (long?)Order.DropEventId).SingleOrDefaultAsync(ct)
            ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido DROP não encontrado.");
        await queue.ExpireAsync(receipt.EnterpriseId, eventId, ct);
        db.ChangeTracker.Clear();
        using var operation = writes.Begin("sales", receipt.EnterpriseId);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(receipt.EnterpriseId, ct);
        var order = await db.DropOrders.IgnoreQueryFilters().Include(Order => Order.DropReservation)
            .ThenInclude(Reservation => Reservation.Lines).ThenInclude(Line => Line.Variant)
            .SingleOrDefaultAsync(Order => Order.Id == receipt.DropOrderId && Order.DropEvent.EnterpriseId == receipt.EnterpriseId, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido DROP não encontrado.");

        if (order.PaymentOperationId != receipt.OperationId || order.CustomerId != receipt.CustomerId ||
            order.TotalAmount != receipt.Amount || receipt.Currency != "BRL" || receipt.Revision < 1 ||
            receipt.Status == "approved" && string.IsNullOrWhiteSpace(receipt.PaymentId))
            throw new SecurityFailure(409, "DropPurchase.PaymentMismatch", "A confirmação não corresponde ao pedido e ao comprador.");

        if (receipt.Revision < order.PaymentRevision)
            return;

        if (receipt.Revision == order.PaymentRevision)
        {
            order.PaymentAccepted(receipt.CheckoutUrl);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return;
        }

        var reservation = order.DropReservation;
        var paid = receipt.Status == "approved";
        var late = paid && (Now >= reservation.ExpiresAt || reservation.DropReservationStatusId != 1);
        var alreadyPaid = order.DropOrderPaymentStatusId == 2;
        string? review = receipt.Status == "review_required" ? "DropPayment.FinancialReview" :
            late && !alreadyPaid ? "DropPayment.LateApproval" : null;

        if (paid && !late && !alreadyPaid)
        {
            foreach (var line in reservation.Lines)
                line.Variant.Commit(line.Quantity);

            reservation.Confirm(Now);
            var participation = await db.DropProducts.IgnoreQueryFilters().SingleAsync(Product => Product.Id == reservation.DropProductId &&
                Product.DropEvent.EnterpriseId == receipt.EnterpriseId, ct);
            participation.RegisterSale(reservation.Quantity);
        }

        order.PaymentAccepted(receipt.CheckoutUrl);
        order.ObservePayment(receipt.Revision, paid && !late && !alreadyPaid, review);
        order.RecordPaymentResolution(receipt.Status);
        order.RecordProviderPayment(receipt.PaymentId);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<object> GetOrderAsync(long orderId, CancellationToken ct)
    {
        var tenant = tenants.GetEnterpriseId();
        var customer = Customer;
        var order = await db.DropOrders.IgnoreQueryFilters().AsNoTracking().Include(Order => Order.ListDropOrderItem)
            .SingleOrDefaultAsync(Order => Order.Id == orderId && Order.CustomerId == customer && Order.DropEvent.EnterpriseId == tenant, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido não encontrado.");

        return new
        {
            order.Id, order.DropEventId, order.DropReservationId, order.DropOrderStatusId, order.DropOrderPaymentStatusId,
            order.TotalAmount, order.BuyerNameSnapshot, order.BuyerEmailSnapshot, order.PaymentCheckoutUrl, order.FinancialReviewCode,
            order.ShippingAddressLine, order.ShippingCity, order.ShippingState, order.ShippingZipCode,
            items = order.ListDropOrderItem.Select(Item => new { Item.ItemName, Item.SKU, Item.Quantity, Item.UnitPrice, Item.TotalPrice, Item.OptionsSnapshotJson })
        };
    }

    public async Task AdvanceFulfillmentAsync(long orderId, long statusId, CancellationToken ct)
    {
        var user = http.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true || user.FindFirst(SecurityNames.Actor)?.Value != "user" || !SecurityNames.HasPermission(user, "orders.manage"))
            throw new SecurityFailure(403, "DropPurchase.ManageRequired", "Permissão de gestão de pedidos necessária.");

        var tenant = tenants.GetEnterpriseId();
        using (writes.Begin("sales", tenant))
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.LockCommercialTenantAsync(tenant, ct);
            var order = await db.DropOrders.IgnoreQueryFilters().SingleOrDefaultAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct)
                ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido não encontrado.");

            if (order.DropOrderPaymentStatusId != 2 || order.FinancialReviewCode is not null || statusId < order.DropOrderStatusId || statusId is < 3 or > 5)
                throw new SecurityFailure(409, "DropPurchase.InvalidTransition", "Transição de entrega inválida.");

            order.AdvanceFulfillment(statusId);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        await AcknowledgeFulfillmentAsync(tenant, orderId, ct);
    }

    public async Task AcknowledgeFulfillmentAsync(long tenant, long orderId, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        var order = await db.DropOrders.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);

        if (!order.PaymentOperationId.HasValue || order.PaymentRevision == 0)
            return;

        var closed = order.DropOrderStatusId is 5 or 6 or 7 && order.FinancialReviewCode is null;
        await payments.AcknowledgeAsync(tenant, order.PaymentOperationId.Value, order.PaymentRevision, closed, order.FinancialReviewCode is not null, ct);

        if (!closed)
            return;

        using var operation = writes.Begin("sales", tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        order = await db.DropOrders.IgnoreQueryFilters().SingleAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);
        order.AcknowledgeFulfillment();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
