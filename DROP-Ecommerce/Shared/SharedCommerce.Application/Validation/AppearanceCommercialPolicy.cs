using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Validation;

public sealed class AppearanceCommercialPolicy(IAppearanceEntitlements entitlements)
{
    public Task<AppearanceCapabilities> GetAsync(long tenant, CancellationToken ct)
    {
        return entitlements.GetAsync(tenant, ct);
    }

    public async Task EnsureAsync(long tenant, ThemeRelease release, string settings, CancellationToken ct)
    {
        var rights = await entitlements.GetAsync(tenant, ct);
        AppearanceCommercialSettings.EnsureAllowed(release, settings, rights);
    }

    public async Task<string> InitialSettingsAsync(long tenant, ThemeRelease release, string settings, CancellationToken ct)
    {
        var rights = await entitlements.GetAsync(tenant, ct);

        if (!AppearanceCommercialSettings.Allows(release, rights))
            throw new AppearanceFailure(409, "Appearance.ThemeNotAllowed", "O tema não está disponível no contrato atual.");

        return AppearanceCommercialSettings.Project(release, settings, rights);
    }
}
