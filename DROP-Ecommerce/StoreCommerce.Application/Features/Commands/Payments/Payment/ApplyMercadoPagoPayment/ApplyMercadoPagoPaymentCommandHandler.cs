using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Enums.Payments;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ApplyMercadoPagoPaymentCommandHandler(
    IProviderNotificationStore store,
    IMercadoPagoPaymentReader gateway,
    IPaymentAttemptStore attempts,
    IMediator mediator)
    : IRequestHandler<ApplyMercadoPagoPaymentCommand>
{
    public async Task Handle(ApplyMercadoPagoPaymentCommand command, CancellationToken ct)
    {
        var connection = command.Connection;
        var paymentId = command.PaymentId;

        var snapshot = new PaymentProviderContext(connection.Provider, connection.Environment, connection.ExternalAccountId, connection.Id);
        var payment = await gateway.GetPaymentAsync(connection.EnterpriseId, snapshot, paymentId, ct);
        var attempt = await store.FindPaymentAttemptAsync(connection, payment.ExternalReference, ct);

        if (attempt is null)
        {
            var drop = await store.FindDropPaymentIntentAsync(connection, payment.ExternalReference, ct);

            if (drop is not null)
                await mediator.SendRequiredAsync(new ReconcileDropSalePaymentCommand(drop.EnterpriseId, drop.OperationId), ct);

            return;
        }

        await store.LinkPaymentAttemptAsync(connection, payment.Id, attempt.Id, ct);
        MercadoPagoPaymentPolicy.ValidatePayment(payment, attempt.ExternalReference, attempt.Amount, attempt.CurrencyCode);
        var verified = new VerifiedExternalPayment(connection.EnterpriseId, connection.Provider, connection.Environment,
            connection.ExternalAccountId, payment.ExternalReference, payment.Id, payment.Amount, payment.CurrencyCode);
        var owner = await attempts.RegisterVerifiedPaymentAsync(verified, ct);
        var status = payment.Status switch
        {
            "pending" or "in_process" => VerifiedPaymentStatus.Pending,
            "authorized" => VerifiedPaymentStatus.Authorized,
            "approved" => VerifiedPaymentStatus.Approved,
            "rejected" => VerifiedPaymentStatus.Rejected,
            "cancelled" => VerifiedPaymentStatus.Cancelled,
            "refunded" => VerifiedPaymentStatus.Refunded,
            _ => throw new SecurityFailure(409, "MercadoPago.StatusNeedsReview", "O estado financeiro exige revisão.")
        };
        long? methodId = payment.MethodId == "pix" ? TransactionMethod.Pix.Id : payment.MethodType switch
        {
            "credit_card" => TransactionMethod.CreditCard.Id,
            "ticket" => TransactionMethod.Boleto.Id,
            "account_money" => TransactionMethod.MercadoPagoBalance.Id,
            _ => null
        };

        if (methodId is null && status is VerifiedPaymentStatus.Approved or VerifiedPaymentStatus.Authorized or VerifiedPaymentStatus.Refunded)
            throw new SecurityFailure(409, "MercadoPago.StatusNeedsReview", "Meio de pagamento ainda não suportado pelo domínio.");

        await mediator.Send(new ApplyVerifiedPaymentCommand(new VerifiedPaymentUpdate(owner, verified, connection.Id, status,
            payment.RefundedAmount, payment.UpdatedAt, methodId)), ct);
        await store.CompleteReviewedNotificationsAsync(connection, payment.Id, ct);
    }
}
