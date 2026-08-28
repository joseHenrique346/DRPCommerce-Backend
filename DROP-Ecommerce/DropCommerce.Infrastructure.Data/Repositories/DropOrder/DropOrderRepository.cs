using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class DropOrderRepository : Repository<DropOrder>, IDropOrderRepository
{
    public DropOrderRepository(DropCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<IEnumerable<DropOrder>> GetByCustomerAsync(long customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DropOrder>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(o => o.DropEventId == dropEventId).ToListAsync(cancellationToken);
    }

    #endregion

    #endregion
}
