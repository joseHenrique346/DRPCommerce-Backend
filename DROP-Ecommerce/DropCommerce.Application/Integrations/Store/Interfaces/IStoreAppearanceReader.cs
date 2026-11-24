using SharedCommerce.Application.Contracts.Appearance;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreAppearanceReader
{
    Task<AppearanceCapabilities> RightsAsync(long enterpriseId, CancellationToken ct);
    Task<MediaAssetReference?> RetainMediaAsync(long enterpriseId, long assetId, CancellationToken ct);
}
