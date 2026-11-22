using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces.Orders;

public interface IDropCouponRepository : IRepository<DropCoupon>
{
    Task<DropCoupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
