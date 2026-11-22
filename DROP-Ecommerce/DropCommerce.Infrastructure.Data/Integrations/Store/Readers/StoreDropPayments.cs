using DropCommerce.Infrastructure.Data.Integrations.Store.Interfaces;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreDropPayments(IStoreEnterpriseRefitApi api, StoreIntegrationScope integration) : IStoreDropPayments
{
    public async Task<DropBuyerProfile> ProfileAsync(long tenant, long customerId, CancellationToken ct)
    {
        using var operation = integration.Begin(tenant);
        using var response = await api.DropBuyerAsync(customerId, ct);
        var profile = StoreIntegrationResponseReader.ReadContentOrNull(response, "Drop buyer");

        if (profile is null || profile.CustomerId != customerId || profile.EnterpriseId != tenant)
            throw new SecurityFailure(503, "DropPayment.BuyerUnavailable", "Não foi possível confirmar o comprador.");

        return profile;
    }

    public async Task<DropSalePaymentReceipt?> StartAsync(long tenant, StartDropSalePayment request, CancellationToken ct)
    {
        using var operation = integration.Begin(tenant);
        using var response = await api.StartDropPaymentAsync(request, ct);

        return StoreIntegrationResponseReader.ReadContentOrNull(response, "Drop payment");
    }

    public async Task<DropSalePaymentReceipt?> GetAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        using var operation = integration.Begin(tenant);
        using var response = await api.DropPaymentAsync(operationId, ct);

        return StoreIntegrationResponseReader.ReadContentOrNull(response, "Drop payment");
    }

    public async Task AcknowledgeAsync(long tenant, Guid operationId, long revision, bool closed, bool requiresReview, CancellationToken ct)
    {
        using var operation = integration.Begin(tenant);
        using var response = await api.AcknowledgeDropPaymentAsync(operationId, new(revision, closed, requiresReview), ct);
        response.EnsureSuccessStatusCode();
    }
}
