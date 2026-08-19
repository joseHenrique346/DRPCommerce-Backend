using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class DropEventRepository : Repository<DropEvent>, IDropEventRepository
{
    public DropEventRepository(DropCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<DropEvent?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<DropEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.DropEventStatusId == 5).ToListAsync(cancellationToken);
    }

    #endregion

    #endregion
}
