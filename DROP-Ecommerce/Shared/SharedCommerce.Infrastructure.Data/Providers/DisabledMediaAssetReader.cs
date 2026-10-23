using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;

namespace SharedCommerce.Infrastructure.Data.Providers;

public sealed class DisabledMediaAssetReader : IMediaAssetReader
{
    public Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct)
    {
        return Task.FromResult<MediaAssetReference?>(null);
    }
}