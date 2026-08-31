using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces;

public interface IDropProductRepository : IRepository<DropProduct>
{
    Task<IEnumerable<DropProduct>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default);
}
