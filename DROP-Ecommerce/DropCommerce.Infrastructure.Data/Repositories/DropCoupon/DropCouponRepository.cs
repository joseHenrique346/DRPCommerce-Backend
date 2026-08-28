using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class DropCouponRepository : Repository<DropCoupon>, IDropCouponRepository
{
    public DropCouponRepository(DropCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<DropCoupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    #endregion

    #endregion
}
