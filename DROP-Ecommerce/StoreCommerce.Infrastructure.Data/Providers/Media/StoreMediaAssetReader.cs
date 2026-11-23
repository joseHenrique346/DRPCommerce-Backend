using SharedCommerce.Application.Interfaces.Media;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Options;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed class StoreMediaAssetReader(StoreCommerceDbContext db, IOptions<SecurityOptions> options) : IMediaAssetReader
{
    public async Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct)
    {
        var asset = await (from Asset in db.MediaAssets.IgnoreQueryFilters().AsNoTracking()
            join Enterprise in db.Enterprises.IgnoreQueryFilters() on Asset.EnterpriseId equals Enterprise.Id
            where Asset.EnterpriseId == enterpriseId && Asset.Id == assetId && !Asset.IsDeleted
            select new { Asset.Id, Asset.PublicId, Enterprise.StorefrontSlug }).SingleOrDefaultAsync(ct);

        if (asset is null || StorefrontUrls.Media(options.Value.StorefrontApiBaseUrl, asset.StorefrontSlug, asset.PublicId) is not { } url)
            return null;

        return new MediaAssetReference(asset.Id, url);
    }
}
