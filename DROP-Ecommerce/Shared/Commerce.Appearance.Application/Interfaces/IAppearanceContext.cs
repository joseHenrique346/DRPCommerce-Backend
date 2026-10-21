using Commerce.Appearance.Contracts;

namespace Commerce.Appearance.Application;

public interface IAppearanceContext
{
    long EnterpriseId { get; }
    long UserId { get; }
    void RequireAdministrator();
    void RequireCatalogPublisher();
}

public interface IAppearanceResources
{
    Task EnsureTargetAsync(long enterpriseId, InputAppearanceTarget target, bool publicAccess, CancellationToken ct);
    Task<bool> ExistsAsync(long enterpriseId, InputAppearanceTarget target, string kind, long id, CancellationToken ct);
}

public interface IMediaAssetReader
{
    Task<MediaAssetReference?> GetAsync(long enterpriseId, long assetId, CancellationToken ct);
}

public sealed record MediaAssetReference(long Id, string Url);
