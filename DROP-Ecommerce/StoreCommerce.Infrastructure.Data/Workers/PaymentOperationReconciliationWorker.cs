using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Infrastructure.Data.Workers;

public sealed class PaymentOperationReconciliationWorker(IServiceScopeFactory scopes, TimeProvider time,
    ILogger<PaymentOperationReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var discovery = scopes.CreateScope();
                var due = await discovery.ServiceProvider.GetRequiredService<IPaymentOperationStore>()
                    .GetDueAsync(time.GetUtcNow().UtcDateTime, stoppingToken);

                foreach (var key in due)
                {
                    try
                    {
                        using var operation = scopes.CreateScope();
                        await operation.ServiceProvider.GetRequiredService<PaymentOperationService>()
                            .ProcessAsync(key.EnterpriseId, key.OperationId, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        logger.LogWarning("Reconciliação da operação {OperationId} indisponível.", key.OperationId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Operações de pagamento indisponíveis. Nova consulta no próximo ciclo.");
            }
        }
    }
}
