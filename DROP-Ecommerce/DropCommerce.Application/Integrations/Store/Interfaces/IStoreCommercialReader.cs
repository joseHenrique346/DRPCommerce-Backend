using SharedCommerce.Application.Contracts.Drop;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreCommercialReader
{
    Task<DropCommercialRights> RightsAsync(long enterpriseId, CancellationToken ct);
}
