using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

namespace StoreCommerce.Infrastructure.Data.Workers;

public sealed class MercadoPagoReconciliationWorker(IServiceScopeFactory scopes, IOptions<MercadoPagoOptions> options,
    TimeProvider time, ILogger<MercadoPagoReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!options.Value.Enabled)
                continue;

            try
            {
                using var discovery = scopes.CreateScope();
                var service = discovery.ServiceProvider.GetRequiredService<MercadoPagoNotifications>();
                await service.ClearExpiredAuthorizationsAsync(stoppingToken);
                var notifications = await service.DueAsync(stoppingToken);

                foreach (var key in notifications)
                    await RunAsync(key, false, stoppingToken);

                var payments = await service.DuePaymentsAsync(stoppingToken);

                foreach (var key in payments)
                    await RunAsync(key, true, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Consulta de pendências Mercado Pago indisponível neste ciclo.");
            }
        }
    }

    private async Task RunAsync(MercadoPagoNotificationKey key, bool reconcilePayment, CancellationToken ct)
    {
        try
        {
            using var operation = scopes.CreateScope();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            var service = operation.ServiceProvider.GetRequiredService<MercadoPagoNotifications>();

            if (reconcilePayment)
                await service.ReconcilePaymentAsync(key.EnterpriseId, key.Id, timeout.Token);
            else
                await service.ProcessAsync(key.EnterpriseId, key.Id, timeout.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogWarning("Operação Mercado Pago {OperationId} será consultada novamente.", key.Id);
        }
    }
}
