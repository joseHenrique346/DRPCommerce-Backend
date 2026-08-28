using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreEnterpriseRefitApi
{
    [Get("/api/enterprises/get-by-id/{enterpriseId}")]
    Task<ApiResponse<StoreApiResponse<StoreEnterpriseResponse>>> GetByIdAsync(long enterpriseId, CancellationToken cancellationToken);
}
