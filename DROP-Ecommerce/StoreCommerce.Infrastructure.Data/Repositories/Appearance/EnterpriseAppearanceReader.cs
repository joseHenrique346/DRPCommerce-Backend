using SharedCommerce.Application.Contracts.Appearance;
using StoreCommerce.Application.Interfaces.Appearance;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class EnterpriseAppearanceReader(StoreCommerceDbContext context) : IEnterpriseAppearanceReader
{
    public Task<AppearanceCapabilities> ReadAsync(long enterpriseId, string channel, CancellationToken ct)
    {
        return context.ReadAppearanceCapabilitiesAsync(enterpriseId, channel, ct);
    }
}
