using DropCommerce.Application.Integrations.Store.Contracts;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreCustomerReader
{
    Task<StoreCustomerData?> GetByIdAsync(long customerId, long enterpriseId, CancellationToken cancellationToken);
    Task<IEnumerable<StoreCustomerData>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken);
}
