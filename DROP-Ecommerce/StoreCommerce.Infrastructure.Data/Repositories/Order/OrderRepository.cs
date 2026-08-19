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

    #endregion

    #endregion
}
