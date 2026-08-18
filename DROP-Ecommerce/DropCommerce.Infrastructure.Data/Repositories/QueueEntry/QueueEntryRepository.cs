using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class QueueEntryRepository : Repository<QueueEntry>, IQueueEntryRepository
{
    public QueueEntryRepository(DropCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<IEnumerable<QueueEntry>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(q => q.DropEventId == dropEventId).ToListAsync(cancellationToken);
    }

    public async Task<QueueEntry?> GetByCustomerAndEventAsync(long customerId, long dropEventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(q => q.CustomerId == customerId && q.DropEventId == dropEventId, cancellationToken);
    }

    #endregion

    #endregion
}
