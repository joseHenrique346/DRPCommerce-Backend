using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Workers;

public sealed class StockReservationExpirationWorker(IServiceScopeFactory scopes, TimeProvider time,
    ILogger<StockReservationExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var discovery = scopes.CreateScope();
                var repository = discovery.ServiceProvider.GetRequiredService<IInventoryRepository>();
                var tenants = await repository.GetExpiredTenantsAsync(time.GetUtcNow().UtcDateTime, stoppingToken);

                foreach (var tenant in tenants)
                {
                    try
                    {
                        using var operation = scopes.CreateScope();
                        var inventory = operation.ServiceProvider.GetRequiredService<InventoryService>();
                        await inventory.ExpireAsync(tenant, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        logger.LogWarning("Expiração de reservas indisponível para a loja {EnterpriseId}.", tenant);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Expiração de reservas de estoque indisponível. Nova tentativa no próximo ciclo.");
            }
        }
    }
}
