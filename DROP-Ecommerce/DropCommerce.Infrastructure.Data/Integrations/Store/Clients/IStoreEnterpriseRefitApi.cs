using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreEnterpriseRefitApi
{
    [Get("/internal/v1/drop/enterprises/{enterpriseId}")]
    Task<ApiResponse<StoreApiResponse<StoreEnterpriseResponse>>> GetByIdAsync(long enterpriseId, CancellationToken cancellationToken);
}
