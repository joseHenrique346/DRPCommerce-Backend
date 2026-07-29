using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreProductRefitApi
{
    [Get("/internal/v1/drop/products/{productId}")]
    Task<ApiResponse<StoreApiResponse<StoreProductResponse>>> GetByIdAsync(long productId, [Query] long enterpriseId, CancellationToken cancellationToken);
}
