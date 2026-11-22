using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Interfaces.Appearance;

public interface IAppearanceResources
{
    Task EnsureTargetAsync(long enterpriseId, InputAppearanceTarget target, bool publicAccess, CancellationToken ct);
    Task<bool> ExistsAsync(long enterpriseId, InputAppearanceTarget target, string kind, long id, CancellationToken ct);

    Task<string> FilterPublishedSettingsAsync(long enterpriseId, string schemaJson, string settingsJson, CancellationToken ct)
    {
        return Task.FromResult(settingsJson);
    }
}
