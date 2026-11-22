using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces.Queue;

public interface IQueueEntryRepository : IRepository<QueueEntry>
{
    Task<IEnumerable<QueueEntry>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default);
    Task<QueueEntry?> GetByCustomerAndEventAsync(long customerId, long dropEventId, CancellationToken cancellationToken = default);
}
