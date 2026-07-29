using DropCommerce.Application.Integrations.Store.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreProductReader(IStoreProductRefitApi api) : IStoreProductReader
{
    public async Task<StoreProductData?> GetByIdAsync(long productId, long enterpriseId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await api.GetByIdAsync(productId, enterpriseId, cancellationToken);
            var product = StoreIntegrationResponseReader.ReadContentOrNull<StoreProductResponse>(response, $"Product {productId}");

            if (product is null || product.EnterpriseId != enterpriseId)
                return null;

            return new StoreProductData(product.ProductId, product.EnterpriseId, product.Name, product.SKU, product.IsActive);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Product {productId}", exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Product {productId}", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Product {productId}", exception);
        }
        catch (ApiException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Product {productId}", exception);
        }
        catch (HttpRequestException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Product {productId}", exception);
        }
    }
}
