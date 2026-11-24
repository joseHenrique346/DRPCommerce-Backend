using SharedCommerce.Application.Contracts.Appearance;

namespace StoreCommerce.Application.Interfaces.Appearance;

public interface IEnterpriseAppearanceReader
{
    Task<AppearanceCapabilities> ReadAsync(long enterpriseId, string channel, CancellationToken ct);
}
