using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Domain.Interfaces.StoreManagement;

public interface IEnterpriseRepository : IRepository<Enterprise>
{
    Task LockAsync(long id, CancellationToken cancellationToken);
    Task<EnterpriseContractState?> GetContractStateAsync(long id, CancellationToken cancellationToken);
}
