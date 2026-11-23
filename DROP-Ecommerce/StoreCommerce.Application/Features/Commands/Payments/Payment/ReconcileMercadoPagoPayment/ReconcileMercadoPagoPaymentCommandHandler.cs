using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReconcileMercadoPagoPaymentCommandHandler(
    IProviderNotificationStore store,
    IMercadoPagoPaymentReader gateway,
    TimeProvider time,
    IMediator mediator)
    : IRequestHandler<ReconcileMercadoPagoPaymentCommand>
{
    public async Task Handle(ReconcileMercadoPagoPaymentCommand command, CancellationToken ct)
    {
        var enterpriseId = command.EnterpriseId;
        var attemptId = command.AttemptId;

        var now = time.GetUtcNow().UtcDateTime;
        var claimed = await store.ClaimPaymentCheckAsync(enterpriseId, attemptId, now, ct);

        if (claimed != 1)
            return;

        var attempt = await store.GetPaymentAttemptAsync(enterpriseId, attemptId, ct);
        var connection = attempt.ProviderConnection ?? throw new InvalidOperationException("Pagamento sem conexão original.");
        var snapshot = new PaymentProviderContext(attempt.Provider, attempt.Environment, attempt.ExternalAccountId, connection.Id);
        var payments = await gateway.SearchPaymentsAsync(enterpriseId, snapshot, attempt.ExternalReference, ct);

        foreach (var payment in payments)
        {
            // A busca pode ter índice defasado. A aplicação sempre consulta o ID antes de decidir o estado financeiro.
            store.ClearTracking();

            try
            {
                await mediator.Send(new ApplyMercadoPagoPaymentCommand(connection, payment.Id), ct);
            }
            catch (SecurityFailure failure) when (failure.Code is "MercadoPago.ScopeMismatch" or "MercadoPago.PaymentMismatch" or
                "Payment.AmountMismatch" or "Payment.ScopeMismatch" or "Payment.PartialRefundNeedsReview" or
                "MercadoPago.StatusNeedsReview" or "MercadoPago.EnvironmentMismatch")
            {
                store.ClearTracking();
                var delivery = MercadoPagoAuthorizationCryptography.Hash("reconciliation:" + payment.Id + ":" + payment.UpdatedAt.Ticks);
                var notification = ProviderNotification.Create(connection, payment.Id, delivery, now, attempt.Id);
                await store.InsertNotificationAsync(notification, detachAfterInsert: false, ct);

                await store.MarkDeliveryForReviewAsync(enterpriseId, connection.Id, delivery, ct);
            }
        }
    }
}
