using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedCommerce.Application.Features.Commands;

namespace SharedCommerce.Infrastructure.Data.Workers;

public sealed class AppearanceMaintenanceWorker(IServiceScopeFactory scopes, ILogger<AppearanceMaintenanceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(new RunAppearanceMaintenanceCommand(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError("Manutenção de aparência falhou: {ExceptionType}. Verifique migrations e pacotes.", exception.GetType().Name);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
