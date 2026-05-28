using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces;

public interface IDropEventRepository : IRepository<DropEvent>
{
    Task<DropEvent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<DropEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default);
}
