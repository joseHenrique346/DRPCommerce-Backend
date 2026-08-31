using DropCommerce.Application.Integrations.Store.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreEmployeeReader(IStoreEmployeeRefitApi api) : IStoreEmployeeReader
{
    public async Task<StoreEmployeeData?> GetByIdAsync(long employeeId, long enterpriseId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await api.GetByIdAsync(employeeId, cancellationToken);
            var employee = StoreIntegrationResponseReader.ReadContentOrNull<StoreEmployeeResponse>(response, $"Employee {employeeId}");

            if (employee is null || employee.EnterpriseId != enterpriseId)
                return null;

            return new StoreEmployeeData(employee.EmployeeId, employee.EnterpriseId, employee.FullName, employee.IsActive);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Employee {employeeId}", exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw StoreIntegrationResponseReader.ToTimeoutException($"Employee {employeeId}", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Employee {employeeId}", exception);
        }
        catch (ApiException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Employee {employeeId}", exception);
        }
        catch (HttpRequestException exception)
        {
            throw StoreIntegrationResponseReader.ToIntegrationException($"Employee {employeeId}", exception);
        }
    }
}
