using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreCustomerRefitApi
{
    [Get("/api/internal/customers/get-by-id/{customerId}")]
    Task<ApiResponse<StoreApiResponse<StoreCustomerResponse>>> GetByIdAsync(long customerId, CancellationToken cancellationToken);

    [Post("/api/internal/customers/get-list-by-list-id")]
    Task<ApiResponse<StoreApiResponse<List<StoreCustomerResponse>>>> GetListByListIdAsync([Body] List<long> ids, CancellationToken cancellationToken);
}
