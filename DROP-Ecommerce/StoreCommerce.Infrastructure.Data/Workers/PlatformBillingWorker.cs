using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoreCommerce.Infrastructure.Data.Billing;

namespace StoreCommerce.Infrastructure.Data.Workers;

public sealed class PlatformBillingWorker(IServiceScopeFactory scopes, IOptions<PlatformBillingOptions> options,
    TimeProvider time, ILogger<PlatformBillingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Conciliação da cobrança da plataforma pendente neste ciclo.");
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var discovery = scopes.CreateScope();
        var due = await discovery.ServiceProvider.GetRequiredService<PlatformBillingService>().DueAsync(ct);

        foreach (var key in due)
        {
            using var scope = scopes.CreateScope();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            var billing = scope.ServiceProvider.GetRequiredService<PlatformBillingService>();

            try
            {
                if (options.Value.Enabled)
                    await billing.ReconcileAsync(key.EnterpriseId, key.Id, timeout.Token);
                else
                    await billing.EnforceExpiryAsync(key.EnterpriseId, key.Id, timeout.Token);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Assinatura da plataforma {SubscriptionId} será consultada novamente.", key.Id);
            }
            finally
            {
                using var persist = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await billing.PostponeAsync(key.EnterpriseId, key.Id, persist.Token);
            }
        }

        if (!options.Value.Enabled)
            return;

        var notifications = await discovery.ServiceProvider.GetRequiredService<PlatformBillingNotifications>().DueAsync(ct);

        foreach (var id in notifications)
        {
            using var scope = scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PlatformBillingNotifications>();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));

            try
            {
                await service.ProcessAsync(id, timeout.Token);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                using var persist = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await service.RetryAsync(id, persist.Token);
                logger.LogWarning("Notificação de cobrança {NotificationId} pendente de conciliação.", id);
            }
        }
    }
}
