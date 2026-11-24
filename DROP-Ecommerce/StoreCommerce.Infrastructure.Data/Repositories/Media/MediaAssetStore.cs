using StoreCommerce.Application.Interfaces.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Options;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Entity;
using StoreCommerce.Application.Media;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class MediaAssetStore(StoreCommerceDbContext db, IOptions<SecurityOptions> options) : IMediaAssetStore
{
    public Task<MediaAsset?> FindAsync(long tenant, long id, CancellationToken ct)
    {
        return db.MediaAssets.IgnoreQueryFilters().SingleOrDefaultAsync(Asset =>
            Asset.Id == id && Asset.EnterpriseId == tenant && !Asset.IsDeleted, ct);
    }

    public Task<MediaAsset?> FindAsync(long tenant, Guid publicId, CancellationToken ct)
    {
        return db.MediaAssets.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(Asset =>
            Asset.PublicId == publicId && Asset.EnterpriseId == tenant && !Asset.IsDeleted, ct);
    }

    public Task<MediaAsset?> FindHashAsync(long tenant, string hash, CancellationToken ct)
    {
        return db.MediaAssets.IgnoreQueryFilters().SingleOrDefaultAsync(Asset =>
            Asset.EnterpriseId == tenant && Asset.ContentHash == hash && !Asset.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<MediaAssetSummary>> ListAsync(long tenant, CancellationToken ct)
    {
        var apiBaseUrl = options.Value.StorefrontApiBaseUrl;

        return await (from Asset in db.MediaAssets.IgnoreQueryFilters().AsNoTracking()
            join Enterprise in db.Enterprises.IgnoreQueryFilters() on Asset.EnterpriseId equals Enterprise.Id
            where Asset.EnterpriseId == tenant && !Asset.IsDeleted
            orderby Asset.Id descending
            select new MediaAssetSummary(Asset.Id, Asset.PublicId, Asset.Width, Asset.Height, Asset.ByteLength,
                Asset.CreatedAt, Asset.IsRetainedByDrop, StorefrontUrls.Media(apiBaseUrl, Enterprise.StorefrontSlug, Asset.PublicId)))
            .Take(100).ToListAsync(ct);
    }

    public async Task<long> BytesAsync(long tenant, CancellationToken ct)
    {
        return await db.MediaAssets.IgnoreQueryFilters().Where(Asset => Asset.EnterpriseId == tenant && !Asset.IsDeleted)
            .SumAsync(Asset => (long?)Asset.ByteLength, ct) ?? 0;
    }

    public async Task<bool> IsReferencedAsync(long tenant, long assetId, CancellationToken ct)
    {
        if (await db.MediaAssets.IgnoreQueryFilters().AnyAsync(Asset => Asset.EnterpriseId == tenant &&
            Asset.Id == assetId && Asset.IsRetainedByDrop, ct))
            return true;

        var revisions = from Revision in db.Set<AppearanceRevision>().IgnoreQueryFilters()
            join Release in db.Set<ThemeRelease>() on Revision.ThemeReleaseId equals Release.Id
            where Revision.EnterpriseId == tenant
            select new { Release.SchemaJson, Revision.SettingsJson };

        await foreach (var revision in revisions.AsAsyncEnumerable().WithCancellation(ct))
        {
            if (AppearanceResourceValidator.References(revision.SchemaJson, revision.SettingsJson).Contains(("asset", assetId)))
                return true;
        }

        var sessions = from Session in db.Set<AppearanceEditSession>().IgnoreQueryFilters()
            join Release in db.Set<ThemeRelease>() on Session.ThemeReleaseId equals Release.Id
            where Session.EnterpriseId == tenant && Session.IsOpen
            select new { Release.SchemaJson, Session.SettingsJson };

        await foreach (var session in sessions.AsAsyncEnumerable().WithCancellation(ct))
        {
            if (AppearanceResourceValidator.References(session.SchemaJson, session.SettingsJson).Contains(("asset", assetId)))
                return true;
        }

        return false;
    }

    public void Add(MediaAsset asset)
    {
        db.MediaAssets.Add(asset);
    }
}
