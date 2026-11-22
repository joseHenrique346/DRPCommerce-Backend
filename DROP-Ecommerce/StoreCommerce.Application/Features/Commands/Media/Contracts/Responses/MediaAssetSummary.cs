using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Media;

public sealed record MediaAssetSummary(long Id, Guid PublicId, int Width, int Height, int ByteLength, DateTime CreatedAt,
    bool IsRetainedByDrop)
{
    public string Url => "/api/storefront/media/" + PublicId;
}
