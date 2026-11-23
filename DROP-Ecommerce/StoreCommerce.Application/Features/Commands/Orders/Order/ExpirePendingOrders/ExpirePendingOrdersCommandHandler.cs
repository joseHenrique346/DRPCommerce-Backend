using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ExpirePendingOrdersCommandHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    OrderWorkflowScope scope,
    TimeProvider time,
    OrderStockWriter stock,
    OrderCouponRedemptionWriter couponUses,
    OrderPaymentOperationWriter paymentOperations)
    : IRequestHandler<ExpirePendingOrdersCommand, int>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<int> Handle(ExpirePendingOrdersCommand command, CancellationToken ct)
    {
        var enterpriseId = command.EnterpriseId;
        var expired = await orders.GetExpiredPaymentOrderIdsAsync(enterpriseId, Now, 50, ct);
        var count = 0;

        foreach (var orderId in expired)
        {
            if (await ExpireOrderAsync(enterpriseId, orderId, ct))
                count++;
        }

        return count;
    }

    private async Task<bool> ExpireOrderAsync(long enterpriseId, long orderId, CancellationToken ct)
    {
        var expired = false;
        long? customerId = null;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);

        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct);

        if (order is null || (customerId.HasValue && order.CustomerId != customerId))
            throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        using (scope.Begin(enterpriseId, customerId))
        {
            await ApplyAsync();
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return expired;

        async Task ApplyAsync()
        {
            // A consulta de candidatos não decide o cancelamento: o pagamento pode ter sido confirmado enquanto aguardávamos.
            var now = Now;

            if (!order.IsAwaitingPayment || order.PaymentExpiresAt is not DateTime expiresAt || expiresAt > now)
                return;

            stock.ReturnStock(order, now);
            await couponUses.ReleaseCouponUseAsync(order, now, ct);
            order.Cancel(OrderPaymentExpiry.ExpiredReason, OrderChangeSource.System, null, now);

            await paymentOperations.EnqueueCancellationsAsync(order, OrderPaymentExpiry.ExpiredReason, OrderChangeSource.System, null, now, ct);
            expired = true;
        }
    }
}
