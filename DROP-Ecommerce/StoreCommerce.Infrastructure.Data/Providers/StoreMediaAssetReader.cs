using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed class StoreMediaAssetReader(StoreCommerceDbContext db) : IMediaAssetReader
{
    public async Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct)
    {
        var asset = await db.MediaAssets.IgnoreQueryFilters().AsNoTracking().Where(Asset =>
            Asset.EnterpriseId == enterpriseId && Asset.Id == assetId && !Asset.IsDeleted)
            .Select(Asset => new { Asset.Id, Asset.PublicId }).SingleOrDefaultAsync(ct);

        return asset is null ? null : new MediaAssetReference(asset.Id, "/api/storefront/media/" + asset.PublicId);
    }
}
