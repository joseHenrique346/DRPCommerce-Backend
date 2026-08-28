using DropCommerce.Application.Integrations.Store.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreCustomerReader(IStoreCustomerRefitApi api) : IStoreCustomerReader
{
    public async Task<StoreCustomerData?> GetByIdAsync(long customerId, long enterpriseId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await api.GetByIdAsync(customerId, cancellationToken);
            var customer = StoreIntegrationResponseReader.ReadContentOrNull<StoreCustomerResponse>(response, $"Customer {customerId}");

            if (customer is null || customer.EnterpriseId != enterpriseId)
                return null;

            return new StoreCustomerData(customer.CustomerId, customer.EnterpriseId, customer.FullName, customer.IsActive);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Customer {customerId}", exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Customer {customerId}", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Customer {customerId}", exception);
        }
        catch (ApiException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Customer {customerId}", exception);
        }
        catch (HttpRequestException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Customer {customerId}", exception);
        }
    }

    public async Task<IEnumerable<StoreCustomerData>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken)
    {
        try
        {
            var response = await api.GetListByListIdAsync(ids, cancellationToken);
            var customers = StoreIntegrationResponseReader.ReadContentOrNull<List<StoreCustomerResponse>>(response, "Customers") ?? [];

            return customers
                .Where(customer => ids.Contains(customer.CustomerId))
                .Select(customer => new StoreCustomerData(customer.CustomerId, customer.EnterpriseId, customer.FullName, customer.IsActive));
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException("Customers", exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException("Customers", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException("Customers", exception);
        }
        catch (ApiException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException("Customers", exception);
        }
        catch (HttpRequestException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException("Customers", exception);
        }
    }
}
