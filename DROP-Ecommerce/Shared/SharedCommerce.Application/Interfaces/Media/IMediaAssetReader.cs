using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Interfaces.Media;

public interface IMediaAssetReader
{
    Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct);
}
