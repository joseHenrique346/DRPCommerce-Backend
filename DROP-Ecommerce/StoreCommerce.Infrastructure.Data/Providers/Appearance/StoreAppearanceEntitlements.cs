using SharedCommerce.Application.Interfaces.Appearance;
using SharedCommerce.Application.Contracts.Appearance;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed class StoreAppearanceEntitlements(StoreCommerceDbContext db) : IAppearanceEntitlements
{
    public Task<AppearanceCapabilities> GetAsync(long enterpriseId, CancellationToken ct)
    {
        return db.ReadAppearanceCapabilitiesAsync(enterpriseId, "store", ct);
    }
}
