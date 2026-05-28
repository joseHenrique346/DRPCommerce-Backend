using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces;

public interface IDropCouponRepository : IRepository<DropCoupon>
{
    Task<DropCoupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
