using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Interfaces.Appearance;

public interface IAppearanceEntitlements
{
    Task<AppearanceCapabilities> GetAsync(long enterpriseId, CancellationToken ct);
}
