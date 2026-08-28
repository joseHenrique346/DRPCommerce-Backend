using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreEmployeeRefitApi
{
    [Get("/api/employees/get-by-id/{employeeId}")]
    Task<ApiResponse<StoreApiResponse<StoreEmployeeResponse>>> GetByIdAsync(long employeeId, CancellationToken cancellationToken);
}
