using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces;

public interface IQueueEntryRepository : IRepository<QueueEntry>
{
    Task<IEnumerable<QueueEntry>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default);
    Task<QueueEntry?> GetByCustomerAndEventAsync(long customerId, long dropEventId, CancellationToken cancellationToken = default);
}
