using Commerce.Appearance.Application;

namespace Commerce.Appearance.Infrastructure;

public sealed class DisabledMediaAssetReader : IMediaAssetReader
{
    public Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct)
    {
        return Task.FromResult<MediaAssetReference?>(null);
    }
}
