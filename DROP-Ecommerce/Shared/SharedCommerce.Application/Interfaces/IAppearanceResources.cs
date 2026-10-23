using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Interfaces;

public interface IAppearanceResources
{
    Task EnsureTargetAsync(long enterpriseId, InputAppearanceTarget target, bool publicAccess, CancellationToken ct);
    Task<bool> ExistsAsync(long enterpriseId, InputAppearanceTarget target, string kind, long id, CancellationToken ct);
}
