using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Interfaces.Payments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StoreCommerce.Infrastructure.Data.Workers;

public sealed class PaymentAttemptReconciliationWorker(IServiceScopeFactory scopes, TimeProvider time,
    ILogger<PaymentAttemptReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var discovery = scopes.CreateScope();
                var store = discovery.ServiceProvider.GetRequiredService<IPaymentAttemptStore>();
                var due = await store.GetDueAsync(time.GetUtcNow().UtcDateTime, 50, stoppingToken);

                foreach (var key in due)
                {
                    try
                    {
                        using var operation = scopes.CreateScope();
                        await operation.ServiceProvider.GetRequiredService<ISender>().Send(new ProcessPaymentAttemptCommand(key.EnterpriseId, key.AttemptId), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        logger.LogWarning("Reconciliação da tentativa {AttemptId} indisponível.", key.AttemptId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Reconciliação de tentativas indisponível. Nova consulta no próximo ciclo.");
            }
        }
    }
}
