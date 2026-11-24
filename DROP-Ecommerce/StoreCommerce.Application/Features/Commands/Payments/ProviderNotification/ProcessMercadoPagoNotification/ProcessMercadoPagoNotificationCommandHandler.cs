using MediatR;
using Microsoft.Extensions.Logging;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProcessMercadoPagoNotificationCommandHandler(
    IProviderNotificationStore store,
    TimeProvider time,
    ILogger<ProcessMercadoPagoNotificationCommandHandler> logger,
    IMediator mediator)
    : IRequestHandler<ProcessMercadoPagoNotificationCommand>
{
    public async Task Handle(ProcessMercadoPagoNotificationCommand command, CancellationToken ct)
    {
        var enterpriseId = command.EnterpriseId;
        var id = command.Id;

        var now = time.GetUtcNow().UtcDateTime;
        var lease = Guid.NewGuid();
        var claimed = await store.ClaimNotificationAsync(enterpriseId, id, now, lease, ct);

        if (claimed != 1)
            return;

        var notification = await store.GetNotificationAsync(enterpriseId, id, ct);
        var status = ProviderNotificationStatus.Completed;
        var code = "Verified";

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            await mediator.Send(new ApplyMercadoPagoPaymentCommand(notification.Connection, notification.PaymentId), timeout.Token);
        }
        catch (SecurityFailure failure) when (failure.Code is "MercadoPago.ScopeMismatch" or "MercadoPago.PaymentMismatch" or
            "Payment.AmountMismatch" or "Payment.ScopeMismatch" or "Payment.PartialRefundNeedsReview" or
            "MercadoPago.StatusNeedsReview" or "MercadoPago.EnvironmentMismatch")
        {
            status = ProviderNotificationStatus.ReviewRequired;
            code = "FinancialReviewRequired";
        }
        catch (Exception)
        {
            status = ProviderNotificationStatus.Pending;
            code = "ProviderOrPersistenceUnavailable";
            logger.LogWarning("Notificação Mercado Pago {NotificationId} pendente de consulta.", id);
        }

        using var persistence = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await store.CompleteNotificationLeaseAsync(enterpriseId, id, lease, status, code, time.GetUtcNow().UtcDateTime.AddMinutes(1), persistence.Token);
    }
}
