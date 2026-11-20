using DropCommerce.Application.Integrations.Store.Interfaces;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;

namespace DropCommerce.Infrastructure.Data.Providers;

public sealed class DropMediaAssetReader(IStoreAppearanceReader store) : IMediaAssetReader
{
    public Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct)
    {
        return store.RetainMediaAsync(enterpriseId, assetId, ct);
    }
}
