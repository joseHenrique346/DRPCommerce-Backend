using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces.Catalog;

public interface IDropProductRepository : IRepository<DropProduct>
{
    Task<IEnumerable<DropProduct>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default);
}
