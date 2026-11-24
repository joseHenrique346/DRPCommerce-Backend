using StoreCommerce.Application.Media;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.Media;

public interface IMediaAssetStore
{
    Task<MediaAsset?> FindAsync(long tenant, long id, CancellationToken ct);
    Task<MediaAsset?> FindAsync(long tenant, Guid publicId, CancellationToken ct);
    Task<MediaAsset?> FindHashAsync(long tenant, string hash, CancellationToken ct);
    Task<IReadOnlyList<MediaAssetSummary>> ListAsync(long tenant, CancellationToken ct);
    Task<long> BytesAsync(long tenant, CancellationToken ct);
    Task<bool> IsReferencedAsync(long tenant, long assetId, CancellationToken ct);
    void Add(MediaAsset asset);
}
