using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Enums.Payments;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ApplyPaymentResultCommandHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    StoreSettingsReader storeSettings,
    OrderWorkflowScope scope,
    TimeProvider time,
    OrderStockWriter stock,
    OrderCouponRedemptionWriter couponUses,
    OrderPaymentOperationWriter paymentOperations)
    : WorkflowRequestHandler<ApplyPaymentResultCommand, OrderDetailResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<OrderDetailResponse> ExecuteAsync(ApplyPaymentResultCommand command, CancellationToken ct)
    {
        var provider = command.Provider;
        var reference = command.Reference;
        var status = command.Status;
        var paidAmount = command.PaidAmount;
        var owner = await orders.FindByPaymentReferenceAsync(provider, reference, ct)
            ?? throw new SecurityFailure(404, "Payment.NotFound", "Cobrança não encontrada.");
        var enterpriseId = owner.EnterpriseId;
        var orderId = owner.OrderId;
        long? customerId = null;
        var source = OrderChangeSource.PaymentGateway;
        var reason = "Pagamento recusado pelo gateway.";

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

        return OrderDetailResponse.FromEntity(order, backoffice: true);

        async Task ApplyAsync()
        {
            if (status == PaymentResultStatus.Failed)
            {
                await RejectAsync();
                return;
            }

            await ApproveAsync();
        }

        async Task RejectAsync()
        {
            if (!order.IsAwaitingPayment)
                return;

            var now = Now;
            stock.ReturnStock(order, now);
            await couponUses.ReleaseCouponUseAsync(order, now, ct);
            order.MarkPaymentFailed(reason, source, null, now);

            foreach (var payment in order.ListTransaction.Where(transaction => transaction.IsPendingPayment))
                payment.MarkFailed();
        }

        async Task ApproveAsync()
        {
            if (paidAmount.HasValue && paidAmount.Value != order.TotalAmount)
                throw new SecurityFailure(409, "Payment.AmountMismatch", "O valor pago é diferente do total do pedido.");

            var payment = order.ListTransaction.Single(Transaction => Transaction.GatewayProvider == provider &&
                Transaction.GatewayReference == reference && Transaction.TransactionTypeId == TransactionType.Payment.Id);
            var now = Now;

            if (order.IsCancelled)
            {
                if (payment.TransactionStatusId == TransactionStatus.Refunded.Id)
                    return;

                if (!payment.IsCapturedPayment)
                    payment.MarkCaptured(reference, now);

                order.RegisterCancelledPaymentCapture(now);
                await paymentOperations.EnqueueRefundsAsync(order, "Pagamento aprovado após o cancelamento do pedido.",
                    OrderChangeSource.PaymentGateway, null, now, ct);

                return;
            }

            if (order.IsPaid)
                return;

            if (!order.IsAwaitingPayment)
                throw new SecurityFailure(409, "Order.NotAwaitingPayment", "O pedido não está aguardando pagamento.");

            var settings = await storeSettings.GetForEnterpriseAsync(owner.EnterpriseId, ct);
            stock.CommitStock(order, settings, now);
            order.ConfirmPayment(settings, OrderChangeSource.PaymentGateway, null, "Pagamento aprovado pelo gateway", now);
            payment.MarkCaptured(reference, now);
            await couponUses.RegisterCouponUseAsync(order, now, ct);
        }
    }
}
