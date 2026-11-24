using StoreCommerce.Application.Payments;
using MediatR;
using Microsoft.Extensions.Logging;
using StoreCommerce.Application.Interfaces.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProcessPaymentAttemptCommandHandler(
    IPaymentAttemptStore store,
    IPaymentGatewayRegistry gateways,
    TimeProvider time,
    ILogger<ProcessPaymentAttemptCommandHandler> logger)
    : IRequestHandler<ProcessPaymentAttemptCommand>
{
    public async Task Handle(ProcessPaymentAttemptCommand command, CancellationToken ct)
    {
        var enterpriseId = command.EnterpriseId;
        var attemptId = command.AttemptId;
        var work = await store.TryClaimAsync(enterpriseId, attemptId, time.GetUtcNow().UtcDateTime, ct);

        if (work is null)
            return;

        var attempt = work.Attempt;
        IPaymentGateway gateway;

        try
        {
            gateway = gateways.Resolve(attempt.Provider);
        }
        catch (Exception)
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
        catch (PaymentNotSubmittedException) when (!work.RecoverOnly)
        {
            using var persistence = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await store.MarkNotSubmittedAsync(work, time.GetUtcNow().UtcDateTime.AddMinutes(1), persistence.Token);
            ct.ThrowIfCancellationRequested();

            return;
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
