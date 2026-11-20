using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Interfaces;

public interface IAppearanceEntitlements
{
    Task<AppearanceCapabilities> GetAsync(long enterpriseId, CancellationToken ct);
}
