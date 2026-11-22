using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces.Events;

public interface IDropEventRepository : IRepository<DropEvent>
{
    Task<DropEvent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<DropEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default);
}
