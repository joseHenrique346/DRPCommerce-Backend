using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces.Catalog;

public interface IServiceRepository : IRepository<Service>
{
    Task<IEnumerable<Service>> GetByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
}
