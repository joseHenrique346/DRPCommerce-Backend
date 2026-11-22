using StoreCommerce.Application.Enums.Payments;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Enums.Payments;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Orders;

public sealed partial class OrderLifecycleService
{
    /// <summary>
    /// Só recebe resultados consultados no provedor, depois de validar conta, conexão, valor, moeda e correlação.
    /// Cada PaymentId tem seu lançamento próprio, mesmo quando uma preferência origina mais de um pagamento.
    /// </summary>
    public Task<Order> ApplyVerifiedPaymentAsync(VerifiedPaymentUpdate update, CancellationToken ct)
    {
        return RunAsync(update.Owner.EnterpriseId, update.Owner.OrderId, null, async order =>
        {
            var attempt = order.ListPaymentAttempt.Single(Attempt => Attempt.Id == update.Owner.AttemptId);
            var verified = update.Payment;

            if (attempt.ProviderConnectionId != update.ProviderConnectionId || attempt.Provider != verified.Provider ||
                attempt.ExternalAccountId != verified.ExternalAccountId || attempt.Environment != verified.Environment ||
                attempt.Amount != verified.Amount || attempt.CurrencyCode != verified.CurrencyCode ||
                attempt.ExternalReference != verified.ExternalReference)
                throw new SecurityFailure(409, "Payment.ScopeMismatch", "O pagamento não corresponde à tentativa original.");

            var external = attempt.Payments.Single(Payment => Payment.PaymentId == verified.PaymentId);

            if (external.ProviderUpdatedAt > update.ProviderUpdatedAt)
                return;

            if (update.RefundedAmount > 0 && update.RefundedAmount != verified.Amount)
                throw new SecurityFailure(409, "Payment.PartialRefundNeedsReview", "Reembolso parcial exige conciliação antes de outra operação.");

            if (update.Status is VerifiedPaymentStatus.Pending or VerifiedPaymentStatus.Rejected or VerifiedPaymentStatus.Cancelled)
            {
                // Uma tentativa recusada não encerra a preferência: o comprador pode usar outro meio de pagamento.
                external.RecordProviderUpdate(update.ProviderUpdatedAt);

                return;
            }

            if (update.Status == VerifiedPaymentStatus.Authorized)
            {
                if (order.IsAwaitingPayment && attempt.Transaction.IsPendingPayment)
                    attempt.Transaction.MarkAuthorized(verified.PaymentId);

                external.RecordProviderUpdate(update.ProviderUpdatedAt);

                return;
            }

            var payment = external.CapturedTransactionId is long capturedId
                ? order.ListTransaction.Single(Transaction => Transaction.Id == capturedId) : attempt.Transaction;
            var additional = external.CapturedTransactionId is null &&
                (payment.IsCapturedPayment || payment.TransactionStatusId == TransactionStatus.Refunded.Id) &&
                payment.GatewayReference != verified.PaymentId;

            if (additional)
            {
                payment = Transaction.CreatePendingPayment(order, attempt.TransactionMethodId, attempt.Provider);
                transactions.Add(payment);
            }

            if (payment.TransactionStatusId == TransactionStatus.Refunded.Id)
            {
                external.RecordProviderUpdate(update.ProviderUpdatedAt);

                return;
            }

            if (!payment.IsCapturedPayment)
            {
                if (update.MethodId.HasValue)
                    payment.SetVerifiedMethod(update.MethodId.Value);

                payment.MarkCaptured(verified.PaymentId, Now);
            }

            external.BindCapture(payment);
            external.RecordProviderUpdate(update.ProviderUpdatedAt);

            if (update.Status == VerifiedPaymentStatus.Refunded)
            {
                if (update.RefundedAmount != verified.Amount)
                    throw new SecurityFailure(409, "Payment.RefundNeedsReview", "O provedor não confirmou o reembolso integral.");

                if (!additional && payment.Id == attempt.TransactionId && !order.IsCancelled && order.CanBeCancelled)
                {
                    ReturnStock(order, Now);
                    await ReleaseCouponUseAsync(order, Now, ct);
                    order.Cancel("Pagamento reembolsado no provedor", OrderChangeSource.PaymentGateway, null, Now);
                }

                transactions.Add(Transaction.CreateRefund(payment, verified.PaymentId, Now));

                if (!order.ListTransaction.Any(Transaction => Transaction.IsCapturedPayment))
                    order.RegisterExternalFullRefund(Now);

                // Aloca IDs e mantém captura, estorno e eventual devolução de estoque na mesma transação.
                await unitOfWork.CommitAsync(ct);

                if (order.IsCancelled && order.ListTransaction.Any(Transaction => Transaction.IsCapturedPayment))
                    await EnqueueRefundsAsync(order, "Pedido reembolsado no provedor", OrderChangeSource.PaymentGateway, null, Now, ct);

                return;
            }

            if (order.IsAwaitingPayment && order.PaymentExpiresAt <= Now)
            {
                ReturnStock(order, Now);
                await ReleaseCouponUseAsync(order, Now, ct);
                order.Cancel(ExpiredReason, OrderChangeSource.PaymentGateway, null, Now);
            }

            if (order.IsCancelled)
            {
                order.RegisterCancelledPaymentCapture(Now);
                await unitOfWork.CommitAsync(ct);
                await EnqueueRefundsAsync(order, "Pagamento aprovado após cancelamento", OrderChangeSource.PaymentGateway, null, Now, ct);

                return;
            }

            if (additional)
            {
                await unitOfWork.CommitAsync(ct);
                await operations.EnqueueAsync(order, payment, PaymentOperationKind.FullRefund, "Pagamento adicional da mesma preferência",
                    OrderChangeSource.PaymentGateway, null, Now, ct);

                return;
            }

            if (order.IsPaid)
                return;

            if (!order.IsAwaitingPayment)
                throw new SecurityFailure(409, "Payment.OrderNeedsReview", "O estado do pedido exige conciliação do pagamento.");

            var settings = await storeSettings.GetForEnterpriseAsync(order.EnterpriseId, ct);
            CommitStock(order, settings, Now);
            order.ConfirmPayment(settings, OrderChangeSource.PaymentGateway, null, "Pagamento confirmado por consulta ao provedor", Now);
            await RegisterCouponUseAsync(order, Now, ct);
        }, ct);
    }
}
