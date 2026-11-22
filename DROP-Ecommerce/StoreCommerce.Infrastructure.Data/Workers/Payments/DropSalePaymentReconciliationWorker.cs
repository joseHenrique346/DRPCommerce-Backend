using SharedCommerce.Application.Result;
using StoreCommerce.Application.Features.Commands;
using MediatR;
using StoreCommerce.Application.Payments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;
using StoreCommerce.Infrastructure.Data.Payments;

namespace StoreCommerce.Infrastructure.Data.Workers;

public sealed class DropSalePaymentReconciliationWorker(IServiceScopeFactory scopes, IOptions<MercadoPagoOptions> options,
    ILogger<DropSalePaymentReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!options.Value.Enabled)
                continue;

            try
            {
                using var discovery = scopes.CreateScope();
                var due = await discovery.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new ListDueDropSalePaymentsQuery(), stoppingToken);

                foreach (var intent in due)
                {
                    using var scope = scopes.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IMediator>();
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(60));

                    try
                    {
                        await service.SendRequiredAsync(new ReconcileDropSalePaymentCommand(intent.EnterpriseId, intent.OperationId), timeout.Token);
                    }
                    catch (Exception) when (!stoppingToken.IsCancellationRequested)
                    {
                        logger.LogWarning("Pagamento de venda DROP {OperationId} aguarda consulta.", intent.OperationId);
                    }
                    finally
                    {
                        using var persist = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        await service.SendRequiredAsync(new PostponeDropSalePaymentCommand(intent.EnterpriseId, intent.OperationId), persist.Token);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Conciliação dos pagamentos DROP pendente neste ciclo.");
            }
        }
    }
}
