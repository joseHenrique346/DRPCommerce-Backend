namespace StoreCommerce.Application.Media;

public sealed record MediaAssetSummary(long Id, Guid PublicId, int Width, int Height, int ByteLength, DateTime CreatedAt,
    bool IsRetainedByDrop, string? Url);
