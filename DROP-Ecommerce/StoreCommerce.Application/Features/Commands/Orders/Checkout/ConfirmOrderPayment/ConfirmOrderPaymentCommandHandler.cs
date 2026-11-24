using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ConfirmOrderPaymentCommandHandler(
    IOrderRepository orders,
    IRepository<Transaction> transactions,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    StoreSettingsReader storeSettings,
    OrderWorkflowScope scope,
    ICurrentIdentity actor,
    TimeProvider time,
    OrderStockWriter stock,
    OrderCouponRedemptionWriter couponUses,
    ITenantProvider tenant)
    : WorkflowRequestHandler<ConfirmOrderPaymentCommand, OrderDetailResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<OrderDetailResponse> ExecuteAsync(ConfirmOrderPaymentCommand command, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var orderId = command.Id;
        long? customerId = null;
        var source = OrderChangeSource.Backoffice;
        long? actorId = actor.UserId;
        var transactionMethodId = command.TransactionMethodId;
        var gatewayReference = command.GatewayReference;
        decimal? paidAmount = null;
        var note = command.Note ?? "Pagamento confirmado manualmente";

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
            if (order.IsPaid)
                return;

            if (!order.IsAwaitingPayment)
                throw new SecurityFailure(409, "Order.NotAwaitingPayment", "O pedido não está aguardando pagamento.");

            if (paidAmount.HasValue && paidAmount.Value != order.TotalAmount)
                throw new SecurityFailure(409, "Payment.AmountMismatch", "O valor pago é diferente do total do pedido.");

            var now = Now;
            var settings = await storeSettings.GetForEnterpriseAsync(enterpriseId, ct);
            var payment = order.ListTransaction.Where(transaction => transaction.IsPendingPayment).OrderByDescending(transaction => transaction.Id).FirstOrDefault();

            if (source == OrderChangeSource.Backoffice &&
                (order.ListTransaction.Any(Transaction => Transaction.GatewayProvider != IntegrationProviders.Manual) ||
                    order.ListPaymentAttempt.Any(Attempt => Attempt.Provider != IntegrationProviders.Manual)))
                throw new SecurityFailure(409, "Payment.ProviderConfirmationRequired",
                    "Pagamentos externos exigem confirmação autenticada do provedor.");

            if (payment is null)
            {
                var method = transactionMethodId ?? order.PaymentMethodId
                    ?? throw new SecurityFailure(400, "Payment.MethodRequired", "Informe a forma de pagamento recebida.");
                payment = Transaction.CreatePendingPayment(order, method, IntegrationProviders.Manual);
                transactions.Add(payment);
            }

            stock.CommitStock(order, settings, now);
            order.ConfirmPayment(settings, source, actorId, note, now);
            payment.MarkCaptured(gatewayReference, now);
            await couponUses.RegisterCouponUseAsync(order, now, ct);
        }
    }
}
