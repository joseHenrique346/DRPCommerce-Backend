using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Workers;

/// <summary>
/// Cancela, a cada minuto, os pedidos pendentes cujo prazo de pagamento (StoreSettings.PendingOrderExpirationMinutes) venceu,
/// liberando estoque, cupom e cobrança. O fluxo revalida o estado e o prazo depois do bloqueio para preservar pagamentos
/// confirmados entre a consulta dos candidatos e o processamento.
/// </summary>
public sealed class PendingOrderExpirationWorker(IServiceScopeFactory scopes, TimeProvider time,
    ILogger<PendingOrderExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1), time);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var discovery = scopes.CreateScope();
                var orders = discovery.ServiceProvider.GetRequiredService<IOrderRepository>();
                var tenants = await orders.GetTenantsWithExpiredPaymentsAsync(time.GetUtcNow().UtcDateTime, stoppingToken);

                foreach (var tenant in tenants)
                {
                    try
                    {
                        using var operation = scopes.CreateScope();
                        await operation.ServiceProvider.GetRequiredService<OrderLifecycleService>().ExpirePendingAsync(tenant, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        logger.LogWarning("Expiração de pedidos pendentes indisponível para a loja {EnterpriseId}.", tenant);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Expiração de pedidos pendentes indisponível. Nova tentativa no próximo ciclo.");
            }
        }
    }
}
