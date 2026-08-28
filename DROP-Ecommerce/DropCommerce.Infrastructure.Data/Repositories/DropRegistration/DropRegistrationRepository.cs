using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class DropRegistrationRepository : Repository<DropRegistration>, IDropRegistrationRepository
{
    public DropRegistrationRepository(DropCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<IEnumerable<DropRegistration>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(r => r.DropEventId == dropEventId).ToListAsync(cancellationToken);
    }

    public async Task<DropRegistration?> GetByCustomerAndEventAsync(long customerId, long dropEventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.CustomerId == customerId && r.DropEventId == dropEventId, cancellationToken);
    }

    #endregion

    #endregion
}
