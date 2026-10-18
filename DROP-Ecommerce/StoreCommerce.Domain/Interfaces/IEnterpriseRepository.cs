using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IEnterpriseRepository : IRepository<Enterprise>
{
    Task LockAsync(long id, CancellationToken cancellationToken);
}
