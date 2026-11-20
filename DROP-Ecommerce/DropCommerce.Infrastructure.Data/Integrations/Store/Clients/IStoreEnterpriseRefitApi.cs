using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreEnterpriseRefitApi
{
    [Get("/api/internal/enterprises/get-by-id/{enterpriseId}")]
    Task<ApiResponse<StoreApiResponse<StoreEnterpriseResponse>>> GetByIdAsync(long enterpriseId, CancellationToken cancellationToken);

    [Get("/api/internal/enterprises/{enterpriseId}/appearance-rights/drop")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Appearance.AppearanceCapabilities>>> AppearanceRightsAsync(
        long enterpriseId, CancellationToken ct);

    [Post("/api/internal/media-assets/{assetId}/retain-for-drop")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Appearance.MediaAssetReference>>> RetainMediaAsync(
        long assetId, CancellationToken ct);
}
