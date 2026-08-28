using DropCommerce.Application.Integrations.Store.Contracts;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreEnterpriseReader
{
    Task<StoreEnterpriseData?> GetByIdAsync(long enterpriseId, CancellationToken cancellationToken);
}
