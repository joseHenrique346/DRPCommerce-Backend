using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreProductRefitApi
{
    [Get("/api/products/get-by-id/{productId}")]
    Task<ApiResponse<StoreApiResponse<StoreProductResponse>>> GetByIdAsync(long productId, CancellationToken cancellationToken);

    [Post("/api/products/get-list-by-list-id")]
    Task<ApiResponse<StoreApiResponse<List<StoreProductResponse>>>> GetListByListIdAsync([Body] List<long> ids, CancellationToken cancellationToken);
}
