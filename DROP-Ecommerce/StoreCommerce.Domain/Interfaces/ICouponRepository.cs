using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
