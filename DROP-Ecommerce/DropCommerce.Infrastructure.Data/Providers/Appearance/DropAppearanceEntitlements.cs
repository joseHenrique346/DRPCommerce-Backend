using SharedCommerce.Application.Interfaces.Appearance;
using DropCommerce.Application.Integrations.Store.Interfaces;
using SharedCommerce.Application.Contracts.Appearance;

namespace DropCommerce.Infrastructure.Data.Providers;

public sealed class DropAppearanceEntitlements(IStoreAppearanceReader store) : IAppearanceEntitlements
{
    public Task<AppearanceCapabilities> GetAsync(long enterpriseId, CancellationToken ct)
    {
        return store.RightsAsync(enterpriseId, ct);
    }
}
