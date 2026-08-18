using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity.Coupon;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    public CouponRepository(StoreCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
    }

    #endregion

    #endregion
}
