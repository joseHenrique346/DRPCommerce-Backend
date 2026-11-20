using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.Exceptions;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreAppearanceReader(IStoreEnterpriseRefitApi api, StoreIntegrationScope integration) : IStoreAppearanceReader
{
    public async Task<AppearanceCapabilities> RightsAsync(long enterpriseId, CancellationToken ct)
    {
        using var operation = integration.Begin(enterpriseId);
        var rights = await ReadAsync(() => api.AppearanceRightsAsync(enterpriseId, ct), "Appearance rights", ct);

        if (rights is null || rights.EnterpriseId != enterpriseId || rights.Channel != "drop" || rights.ContractRevision <= 0)
            throw new AppearanceFailure(503, "Appearance.ContractUnavailable", "Não foi possível verificar o contrato atual.");

        return rights;
    }

    public async Task<MediaAssetReference?> RetainMediaAsync(long enterpriseId, long assetId, CancellationToken ct)
    {
        using var operation = integration.Begin(enterpriseId);
        return await ReadAsync(() => api.RetainMediaAsync(assetId, ct), "Appearance media", ct);
    }

    private static async Task<T?> ReadAsync<T>(Func<Task<ApiResponse<StoreApiResponse<T>>>> read, string resource, CancellationToken ct)
    {
        try
        {
            using var response = await read();

            return StoreIntegrationResponseReader.ReadContentOrNull(response, resource);
        }
        catch (OperationCanceledException exception) when (!ct.IsCancellationRequested)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException(resource, exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException(resource, exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException(resource, exception);
        }
        catch (ApiException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException(resource, exception);
        }
        catch (HttpRequestException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException(resource, exception);
        }
    }
}
