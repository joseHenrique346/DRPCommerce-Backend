using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Payments;
using Microsoft.Extensions.Logging;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Payments;

public sealed class PaymentOperationService(IPaymentOperationStore store, IPaymentGatewayRegistry gateways,
    ILogger<PaymentOperationService> logger, TimeProvider time)
{
    public async Task ProcessAsync(long enterpriseId, long operationId, CancellationToken ct)
    {
        var work = await store.TryClaimAsync(enterpriseId, operationId, time.GetUtcNow().UtcDateTime, ct);

        if (work is null)
            return;

        var operation = work.Operation;
        PaymentOperationResult result;

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            var request = new PaymentOperationRequest(enterpriseId, operation.OrderId, operation.Kind,
                new PaymentProviderContext(operation.Provider, operation.Environment!.Value,
                    operation.ExternalAccountId!, operation.ProviderConnectionId), operation.IdempotencyKey,
                operation.ExternalReference, operation.PaymentId, operation.PreferenceId, operation.Amount,
                operation.CurrencyCode, operation.Reason);

            var gateway = gateways.Resolve(operation.Provider);

            if (work.RecoverOnly)
                result = await gateway.RecoverOperationAsync(request, timeout.Token);
            else
                result = await gateway.ExecuteOperationAsync(request, timeout.Token);

            if (result.Status is not (PaymentOperationStatus.Succeeded or PaymentOperationStatus.Pending or
                PaymentOperationStatus.Failed or PaymentOperationStatus.OutcomeUnknown or
                PaymentOperationStatus.AwaitingManualConfirmation or PaymentOperationStatus.ReviewRequired) ||
                result.Status == PaymentOperationStatus.Succeeded && string.IsNullOrWhiteSpace(result.Reference) ||
                result.Reference?.Length > 200 ||
                result.Status == PaymentOperationStatus.AwaitingManualConfirmation &&
                    (operation.Provider != "manual" || operation.Kind != PaymentOperationKind.FullRefund))
                result = new PaymentOperationResult(PaymentOperationStatus.OutcomeUnknown);
        }
        catch (PaymentNotSubmittedException) when (!work.RecoverOnly)
        {
            result = new PaymentOperationResult(PaymentOperationStatus.Failed);
        }
        catch (Exception)
        {
            logger.LogWarning("Resultado da operação de pagamento {OperationId} indisponível; será consultado.", operationId);
            result = new PaymentOperationResult(PaymentOperationStatus.OutcomeUnknown);
        }

        // Mesmo se o cliente/host cancelar, tenta registrar a incerteza. Falha de gravação conserva o lease para consulta.
        using var persistence = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await store.CompleteAsync(work, result, persistence.Token);
    }
}
