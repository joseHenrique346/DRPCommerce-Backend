using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;
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

    public Task<Order?> GetForWorkflowAsync(long enterpriseId, long orderId, CancellationToken cancellationToken)
    {
        return _context.Orders
            .IgnoreQueryFilters()
            .Include(e => e.ListOrderItem).ThenInclude(item => item.ProductVariant!).ThenInclude(variant => variant.Product)
            .Include(e => e.Coupon)
            .Include(e => e.StockReservation!).ThenInclude(reservation => reservation.Lines).ThenInclude(line => line.ProductVariant)
            .Include(e => e.ListTransaction)
            .Include(e => e.ListStatusHistory)
            .Include(e => e.ListShipment)
            .Where(e => e.EnterpriseId == enterpriseId && e.Id == orderId && !e.IsDeleted)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<long> GetNextOrderNumberAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        var last = await _context.Orders.IgnoreQueryFilters()
            .Where(e => e.EnterpriseId == enterpriseId)
            .MaxAsync(e => (long?)e.OrderNumber, cancellationToken);

        return (last ?? 0) + 1;
    }

    public async Task<(List<Order> Items, int TotalCount)> SearchAsync(StoreCommerce.Domain.Models.OrderSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking();

        if (criteria.OrderNumber.HasValue)
            query = query.Where(e => e.OrderNumber == criteria.OrderNumber);

        if (criteria.StatusIds is { Count: > 0 } statuses)
            query = query.Where(e => statuses.Contains(e.OrderStatusId));

        if (criteria.PaymentStatusIds is { Count: > 0 } paymentStatuses)
            query = query.Where(e => paymentStatuses.Contains(e.OrderPaymentStatusId));

        if (criteria.CustomerId.HasValue)
            query = query.Where(e => e.CustomerId == criteria.CustomerId);

        if (criteria.CouponId.HasValue)
            query = query.Where(e => e.CouponId == criteria.CouponId);

        if (criteria.CreatedFrom.HasValue)
            query = query.Where(e => e.CreatedAt >= criteria.CreatedFrom);

        if (criteria.CreatedTo.HasValue)
            query = query.Where(e => e.CreatedAt <= criteria.CreatedTo);

        if (criteria.MinTotal.HasValue)
            query = query.Where(e => e.TotalAmount >= criteria.MinTotal);

        if (criteria.MaxTotal.HasValue)
            query = query.Where(e => e.TotalAmount <= criteria.MaxTotal);

        var count = await query.CountAsync(cancellationToken);
        var ordered = criteria.Sort switch
        {
            "oldest" => query.OrderBy(e => e.OrderNumber).ThenBy(e => e.Id),
            "total_desc" => query.OrderByDescending(e => e.TotalAmount).ThenByDescending(e => e.Id),
            "total_asc" => query.OrderBy(e => e.TotalAmount).ThenBy(e => e.Id),
            _ => query.OrderByDescending(e => e.OrderNumber).ThenByDescending(e => e.Id)
        };
        var items = await ordered.Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize).ToListAsync(cancellationToken);

        return (items, count);
    }

    public Task<Order?> FindByIdempotencyKeyAsync(long enterpriseId, long customerId, string idempotencyKey, CancellationToken cancellationToken)
    {
        return _context.Orders.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(e => e.EnterpriseId == enterpriseId && e.CustomerId == customerId && e.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task<List<long>> GetTenantsWithExpiredPaymentsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var pending = OrderStatus.Pending.Id;

        return _context.Orders.IgnoreQueryFilters().AsNoTracking()
            .Where(e => e.OrderStatusId == pending && e.PaymentExpiresAt != null && e.PaymentExpiresAt <= now && !e.IsDeleted)
            .Select(e => e.EnterpriseId).Distinct().OrderBy(id => id).Take(200)
            .ToListAsync(cancellationToken);
    }

    public Task<List<long>> GetExpiredPaymentOrderIdsAsync(long enterpriseId, DateTime now, int take, CancellationToken cancellationToken)
    {
        var pending = OrderStatus.Pending.Id;

        return _context.Orders.IgnoreQueryFilters().AsNoTracking()
            .Where(e => e.EnterpriseId == enterpriseId && e.OrderStatusId == pending && e.PaymentExpiresAt != null &&
                e.PaymentExpiresAt <= now && !e.IsDeleted)
            .OrderBy(e => e.PaymentExpiresAt).Select(e => e.Id).Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<(long EnterpriseId, long OrderId)?> FindByPaymentReferenceAsync(string provider, string reference, CancellationToken cancellationToken)
    {
        var payment = TransactionType.Payment.Id;
        var match = await _context.Transactions.IgnoreQueryFilters().AsNoTracking()
            .Where(e => e.GatewayProvider == provider && e.GatewayReference == reference && e.TransactionTypeId == payment)
            .Select(e => new { e.Order.EnterpriseId, e.OrderId })
            .FirstOrDefaultAsync(cancellationToken);

        return match is null ? null : (match.EnterpriseId, match.OrderId);
    }

    public Task<Order?> GetDetailAsync(long orderId, long? customerId, CancellationToken cancellationToken)
    {
        return _dbSet.AsNoTracking()
            .Include(e => e.ListOrderItem)
            .Include(e => e.ListTransaction)
            .Include(e => e.ListStatusHistory)
            .Include(e => e.ListShipment)
            .Where(e => e.Id == orderId && (customerId == null || e.CustomerId == customerId))
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetPageForCustomerAsync(long customerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsNoTracking().Where(e => e.CustomerId == customerId);
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(e => e.OrderNumber).ThenByDescending(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, count);
    }

    public async Task<(int CompletedOrders, decimal TotalSpent)> GetCustomerPurchaseSummaryAsync(long customerId, long? excludeOrderId, CancellationToken cancellationToken = default)
    {
        var statuses = Order.CompletedStatusIds.ToList();
        var summary = await _dbSet.AsNoTracking()
            .Where(e => e.CustomerId == customerId && statuses.Contains(e.OrderStatusId) && (excludeOrderId == null || e.Id != excludeOrderId))
            .GroupBy(e => e.CustomerId)
            .Select(group => new { Count = group.Count(), Total = group.Sum(e => e.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        return summary is null ? (0, 0) : (summary.Count, summary.Total);
    }

    #endregion

    #endregion
}
