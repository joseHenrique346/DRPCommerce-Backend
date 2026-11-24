using SharedCommerce.Application.Result;
using DropCommerce.Application.Features.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DropCommerce.Infrastructure.Data.Workers;

public sealed class DropQueueExpirationWorker(IServiceScopeFactory scopes, ILogger<DropQueueExpirationWorker> logger) : BackgroundService
{
    private long cursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var discovery = scopes.CreateScope();
                var events = await discovery.ServiceProvider.GetRequiredService<IMediator>()
                    .Send(new ListDropQueueWorkQuery(cursor), stoppingToken);
                cursor = events.Count == 0 ? 0 : events[^1].DropEventId;

                foreach (var drop in events)
                {
                    try
                    {
                        using var scope = scopes.CreateScope();
                        await scope.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new AdmitDropQueueCommand(drop.EnterpriseId, drop.DropEventId), stoppingToken);
                    }
                    catch (Exception) when (!stoppingToken.IsCancellationRequested)
                    {
                        logger.LogWarning("Fila DROP {EventId} aguarda nova conciliação.", drop.DropEventId);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Conciliação das filas DROP pendente neste ciclo.");
            }
        }
    }
}
