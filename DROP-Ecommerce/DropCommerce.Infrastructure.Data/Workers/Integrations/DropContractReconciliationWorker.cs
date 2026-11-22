using SharedCommerce.Application.Result;
using DropCommerce.Application.Features.Commands;
using MediatR;
using DropCommerce.Application.Contracts;
using System.Net.Http.Json;
using DropCommerce.Infrastructure.Data.Context;
using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Options;

namespace DropCommerce.Infrastructure.Data.Workers;

public sealed class DropContractReconciliationWorker(IServiceScopeFactory scopes, IHttpClientFactory clients,
    IOptions<SecurityOptions> options, ILogger<DropContractReconciliationWorker> logger) : BackgroundService
{
    private long orderCursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning("Sincronização dos contratos DROP pendente neste ciclo.");
            }
        }
    }

    private async Task ReconcileAsync(CancellationToken ct)
    {
        using var client = clients.CreateClient("DropContractSync");
        client.DefaultRequestHeaders.Add("X-Service-Key", options.Value.InternalServiceKey);
        var pending = await client.GetFromJsonAsync<StoreApiResponse<List<DropCommercialRights>>>("/api/internal/drop-contracts/pending", ct);

        if (pending is not { IsSuccess: true, Content: not null } || pending.Content.Count > 50)
            throw new InvalidOperationException("Lote de contratos inválido.");

        foreach (var rights in pending.Content)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<DropCommerceDbContext>().ApplyCommercialRightsAsync(rights, ct);
                using var acknowledgment = new HttpRequestMessage(HttpMethod.Post,
                    $"/api/internal/enterprises/{rights.EnterpriseId}/drop-contract/ack");
                acknowledgment.Headers.Add("X-Enterprise-Id", rights.EnterpriseId.ToString(System.Globalization.CultureInfo.InvariantCulture));
                acknowledgment.Content = JsonContent.Create(new { revision = rights.ContractRevision });
                using var response = await client.SendAsync(acknowledgment, ct);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Contrato DROP da loja {EnterpriseId} permanece pendente de confirmação.", rights.EnterpriseId);
            }
        }

        var stock = await client.GetFromJsonAsync<StoreApiResponse<List<DropStockExport>>>("/api/internal/drop-stock-transfers/pending", ct);

        if (stock is not { IsSuccess: true, Content: not null } || stock.Content.Count > 50)
            throw new InvalidOperationException("Lote de estoque inválido.");

        foreach (var export in stock.Content)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var receipt = await scope.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new ImportDropStockCommand(export), ct);

                if (receipt.Accepted)
                {
                    await SendAsync(client, $"/api/internal/drop-stock-transfers/{export.OperationId}/ack", export.EnterpriseId,
                        new { catalogProductId = receipt.CatalogProductId }, ct);
                }
                else
                {
                    // A rejeição durável impede uma importação tardia depois de restaurar a origem.
                    await SendAsync(client, "/api/internal/drop-stock-transfers/return", export.EnterpriseId,
                        new DropStockReturn(export.OperationId, export.OperationId, export.EnterpriseId, export.ProductId, export.Variants), ct);
                }
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Transferência {OperationId} permanece pendente, sem duplicar saldo.", export.OperationId);
            }
        }

        using var discovery = scopes.CreateScope();
        var returns = await discovery.ServiceProvider.GetRequiredService<DropCommerceDbContext>().Set<DropStockReturnIntent>()
            .IgnoreQueryFilters().AsNoTracking().Where(Return => Return.CompletedAt == null)
            .OrderBy(Return => Return.Id).Take(50).Select(Return => Return.SnapshotJson).ToListAsync(ct);

        foreach (var snapshot in returns)
        {
            var returned = JsonSerializer.Deserialize<DropStockReturn>(snapshot)!;

            try
            {
                await SendAsync(client, "/api/internal/drop-stock-transfers/return", returned.EnterpriseId, returned, ct);
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new CompleteDropStockReturnCommand(returned.EnterpriseId, returned.OperationId), ct);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Retorno {OperationId} permanece pendente de confirmação.", returned.OperationId);
            }
        }

        var payments = await client.GetFromJsonAsync<StoreApiResponse<List<DropSalePaymentReceipt>>>("/api/internal/drop-sale-payments/pending-receipts", ct);

        if (payments is not { IsSuccess: true, Content: not null } || payments.Content.Count > 50)
            throw new InvalidOperationException("Lote de confirmações de pagamento inválido.");

        foreach (var receipt in payments.Content)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var checkout = scope.ServiceProvider.GetRequiredService<IMediator>();
                await checkout.SendRequiredAsync(new ApplyDropPaymentReceiptCommand(receipt), ct);
                await checkout.SendRequiredAsync(new AcknowledgeDropFulfillmentCommand(receipt.EnterpriseId, receipt.DropOrderId), ct);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Confirmação de pagamento DROP {OperationId} permanece pendente.", receipt.OperationId);
            }
        }

        var unfinished = await discovery.ServiceProvider.GetRequiredService<DropCommerceDbContext>().DropOrders.IgnoreQueryFilters().AsNoTracking()
            .Where(Order => Order.Id > orderCursor && Order.PaymentOperationId != null && (!Order.PaymentIntentAccepted ||
                Order.DropOrderStatusId >= 5 && !Order.FulfillmentAcknowledged))
            .OrderBy(Order => Order.Id).Take(50).Select(Order => new { Order.Id, Order.DropEvent.EnterpriseId, Order.PaymentIntentAccepted }).ToListAsync(ct);
        orderCursor = unfinished.Count == 0 ? 0 : unfinished[^1].Id;

        foreach (var order in unfinished)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var checkout = scope.ServiceProvider.GetRequiredService<IMediator>();

                if (!order.PaymentIntentAccepted)
                    await checkout.SendRequiredAsync(new ReconcileDropPurchaseCommand(order.EnterpriseId, order.Id), ct);
                else
                    await checkout.SendRequiredAsync(new AcknowledgeDropFulfillmentCommand(order.EnterpriseId, order.Id), ct);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Pedido DROP {OrderId} aguarda recuperação do pagamento ou confirmação de entrega.", order.Id);
            }
        }
    }

    private static async Task SendAsync(HttpClient client, string path, long tenant, object body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("X-Enterprise-Id", tenant.ToString(System.Globalization.CultureInfo.InvariantCulture));
        request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
