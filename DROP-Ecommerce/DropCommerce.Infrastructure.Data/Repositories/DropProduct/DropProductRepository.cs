using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class DropProductRepository : Repository<DropProduct>, IDropProductRepository
{
    public DropProductRepository(DropCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<IEnumerable<DropProduct>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(p => p.DropEventId == dropEventId).ToListAsync(cancellationToken);
    }

    #endregion

    #endregion
}
