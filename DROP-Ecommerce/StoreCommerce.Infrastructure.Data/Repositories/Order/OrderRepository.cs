using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(StoreCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<IEnumerable<Order>> GetByCustomerAsync(long customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CustomerId == customerId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(long statusId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.OrderStatusId == statusId).ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetListWithItemsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
    {
        var orders = await _dbSet
            .Include(e => e.ListOrderItem)
            .Include(e => e.Coupon)
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);

        // Cupons excluídos logicamente são ocultados pelo filtro global, mas continuam valendo para pedidos que já os utilizam.
        // Carregá-los no mesmo contexto faz o EF preencher a navegação Order.Coupon automaticamente.
        var missingCouponIds = orders
            .Where(e => e.CouponId.HasValue && e.Coupon is null)
            .Select(e => e.CouponId!.Value)
            .Distinct()
            .ToList();

        if (missingCouponIds.Count > 0)
        {
            var enterpriseIds = orders.Select(e => e.EnterpriseId).Distinct().ToList();

            await _context.Coupons
                .IgnoreQueryFilters()
                .Where(e => missingCouponIds.Contains(e.Id) && enterpriseIds.Contains(e.EnterpriseId))
                .ToListAsync(cancellationToken);
        }

        return orders;
    }

    #endregion

    #endregion
}
