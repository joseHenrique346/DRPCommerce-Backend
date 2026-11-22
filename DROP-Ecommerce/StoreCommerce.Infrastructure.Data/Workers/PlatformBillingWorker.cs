using StoreCommerce.Application.Result;
using StoreCommerce.Application.Features.Commands;
using MediatR;
using StoreCommerce.Application.Billing;
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
        var due = await discovery.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new ListDuePlatformSubscriptionsQuery(), ct);

        foreach (var key in due)
        {
            using var scope = scopes.CreateScope();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            var billing = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                if (options.Value.Enabled)
                    await billing.SendRequiredAsync(new ReconcilePlatformSubscriptionCommand(key.EnterpriseId, key.Id), timeout.Token);
                else
                    await billing.SendRequiredAsync(new EnforcePlatformSubscriptionExpiryCommand(key.EnterpriseId, key.Id), timeout.Token);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Assinatura da plataforma {SubscriptionId} será consultada novamente.", key.Id);
            }
            finally
            {
                using var persist = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await billing.SendRequiredAsync(new PostponePlatformSubscriptionCommand(key.EnterpriseId, key.Id), persist.Token);
            }
        }

        if (!options.Value.Enabled)
            return;

        var notifications = await discovery.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new ListDuePlatformBillingNotificationsQuery(), ct);

        foreach (var id in notifications)
        {
            using var scope = scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediator>();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));

            try
            {
                await service.SendRequiredAsync(new ProcessPlatformBillingNotificationCommand(id), timeout.Token);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                using var persist = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await service.SendRequiredAsync(new RetryPlatformBillingNotificationCommand(id), persist.Token);
                logger.LogWarning("Notificação de cobrança {NotificationId} pendente de conciliação.", id);
            }
        }
    }
}
