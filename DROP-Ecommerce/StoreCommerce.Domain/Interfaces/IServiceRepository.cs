using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<IEnumerable<Service>> GetByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
}
