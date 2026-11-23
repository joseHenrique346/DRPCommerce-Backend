using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    OrderWorkflowScope scope,
    ICurrentIdentity actor,
    TimeProvider time,
    OrderStockWriter stock,
    OrderCouponRedemptionWriter couponUses,
    OrderPaymentOperationWriter paymentOperations,
    ITenantProvider tenant)
    : WorkflowRequestHandler<CancelOrderCommand, OrderDetailResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<OrderDetailResponse> ExecuteAsync(CancelOrderCommand command, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var orderId = command.Id;
        long? customerId = null;
        var source = OrderChangeSource.Backoffice;
        long? actorId = actor.UserId;
        var reason = command.Reason.Trim();

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

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(command.Id, null, ct))!, backoffice: true);

        async Task ApplyAsync()
        {
            if (order.IsCancelled)
                return;

            if (customerId.HasValue && !order.IsAwaitingPayment)
                throw new SecurityFailure(409, "Order.CancelNotAllowed",
                    "Pedidos já pagos só podem ser cancelados pela loja. Entre em contato com o atendimento.");

            if (!order.CanBeCancelled)
                throw new SecurityFailure(409, "Order.CancelNotAllowed", "Pedidos enviados ou entregues não podem ser cancelados.");

            var now = Now;
            var wasPaid = order.IsPaid;

            if (source == OrderChangeSource.Backoffice)
            {
                actor.RequirePermission(PermissionCodes.OrdersCancel);

                if (wasPaid)
                    actor.RequirePermission(PermissionCodes.OrdersRefund);
            }

            stock.ReturnStock(order, now);
            await couponUses.ReleaseCouponUseAsync(order, now, ct);
            order.Cancel(reason, source, actorId, now);

            if (wasPaid)
                await paymentOperations.EnqueueRefundsAsync(order, reason, source, actorId, now, ct);
            else
                await paymentOperations.EnqueueCancellationsAsync(order, reason, source, actorId, now, ct);
        }
    }
}
