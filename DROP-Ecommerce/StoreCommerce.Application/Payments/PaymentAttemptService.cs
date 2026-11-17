using Microsoft.Extensions.Logging;

namespace StoreCommerce.Application.Payments;

public sealed class PaymentAttemptService(IPaymentAttemptStore store, IPaymentGateway gateway,
    TimeProvider time, ILogger<PaymentAttemptService> logger)
{
    public async Task ProcessAsync(long enterpriseId, long attemptId, CancellationToken ct)
    {
        var work = await store.TryClaimAsync(enterpriseId, attemptId, time.GetUtcNow().UtcDateTime, ct);

        if (work is null)
            return;

        var attempt = work.Attempt;

        if (attempt.Provider != gateway.Provider)
        {
            await MarkUnknownAsync(work);

            return;
        }

        var request = new PaymentRequest(attempt.EnterpriseId, attempt.OrderId, attempt.CustomerId,
            attempt.CustomerName, attempt.CustomerEmail, attempt.Amount, attempt.CurrencyCode,
            attempt.TransactionMethodId, attempt.ExpiresAt, attempt.IdempotencyKey,
            new PaymentProviderContext(attempt.Provider, attempt.Environment, attempt.ExternalAccountId,
                attempt.ProviderConnectionId), attempt.ExternalReference);
        PaymentSession? session;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));

        try
        {
            // Processing vencido/OutcomeUnknown sempre consulta. null mantém a pendência, sem repetir criação.
            session = work.RecoverOnly
                ? await gateway.RecoverPaymentAsync(request, timeout.Token)
                : await gateway.CreatePaymentAsync(request, timeout.Token);
        }
        catch (Exception)
        {
            // Não registrar a exceção do SDK: sua mensagem pode conter tokens ou dados pessoais.
            logger.LogWarning("Resultado da criação/consulta do pagamento {AttemptId} desconhecido.", attempt.Id);
            await MarkUnknownAsync(work);
            ct.ThrowIfCancellationRequested();

            return;
        }

        if (session is null)
        {
            await MarkUnknownAsync(work);

            return;
        }

        // Se a gravação falhar, a lease vence e a próxima execução consulta a mesma referência.
        await store.CompleteAsync(work, session, ct);
    }

    private async Task MarkUnknownAsync(PaymentAttemptWork work)
    {
        using var recovery = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await store.MarkUnknownAsync(work, time.GetUtcNow().UtcDateTime.AddMinutes(1), recovery.Token);
    }
}
