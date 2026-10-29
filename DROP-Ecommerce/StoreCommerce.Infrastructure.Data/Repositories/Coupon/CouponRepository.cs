using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
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
        var normalized = Coupon.NormalizeCode(code);

        return await _dbSet.FirstOrDefaultAsync(e => e.Code == normalized, cancellationToken);
    }

    public async Task<List<string>> GetExistingCodesAsync(IReadOnlyCollection<string> codes, IReadOnlyCollection<long> excludeIds, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(e => codes.Contains(e.Code) && !excludeIds.Contains(e.Id))
            .Select(e => e.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Coupon> Items, int TotalCount)> SearchAsync(
        string? code, long? couponTypeId, bool? isActive, CouponAvailability? availability, DateTime now,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var term = Coupon.NormalizeCode(code);
            query = query.Where(e => e.Code.Contains(term));
        }

        if (couponTypeId.HasValue)
            query = query.Where(e => e.CouponTypeId == couponTypeId);

        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive);

        // Mesma precedência de Coupon.GetAvailability: inativo, expirado, esgotado, agendado, vigente.
        query = availability switch
        {
            CouponAvailability.Inactive => query.Where(e => !e.IsActive),
            CouponAvailability.Expired => query.Where(e => e.IsActive && e.ExpiresAt <= now),
            CouponAvailability.Exhausted => query.Where(e => e.IsActive && e.ExpiresAt > now && e.MaxUses != null && e.UsedCount >= e.MaxUses),
            CouponAvailability.Scheduled => query.Where(e => e.IsActive && e.ExpiresAt > now && (e.MaxUses == null || e.UsedCount < e.MaxUses) && e.StartsAt > now),
            CouponAvailability.Active => query.Where(e => e.IsActive && e.ExpiresAt > now && (e.MaxUses == null || e.UsedCount < e.MaxUses) && e.StartsAt <= now),
            _ => query
        };

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, count);
    }

    #endregion

    #endregion
}
