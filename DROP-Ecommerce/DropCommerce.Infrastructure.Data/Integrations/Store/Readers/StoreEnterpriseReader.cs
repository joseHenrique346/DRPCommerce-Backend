using DropCommerce.Application.Integrations.Store.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreEnterpriseReader(IStoreEnterpriseRefitApi api) : IStoreEnterpriseReader
{
    public async Task<StoreEnterpriseData?> GetByIdAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await api.GetByIdAsync(enterpriseId, cancellationToken);
            var enterprise = StoreIntegrationResponseReader.ReadContentOrNull<StoreEnterpriseResponse>(response, $"Enterprise {enterpriseId}");

            return enterprise is null || enterprise.EnterpriseId != enterpriseId
                ? null
                : new StoreEnterpriseData(enterprise.EnterpriseId, enterprise.TradeName, enterprise.IsActive);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Enterprise {enterpriseId}", exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Enterprise {enterpriseId}", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Enterprise {enterpriseId}", exception);
        }
        catch (ApiException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Enterprise {enterpriseId}", exception);
        }
        catch (HttpRequestException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Enterprise {enterpriseId}", exception);
        }
    }
}
