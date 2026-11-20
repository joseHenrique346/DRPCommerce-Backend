using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed class StoreAppearanceEntitlements(StoreCommerceDbContext db) : IAppearanceEntitlements
{
    public Task<AppearanceCapabilities> GetAsync(long enterpriseId, CancellationToken ct)
    {
        return db.ReadAppearanceCapabilitiesAsync(enterpriseId, "store", ct);
    }
}
