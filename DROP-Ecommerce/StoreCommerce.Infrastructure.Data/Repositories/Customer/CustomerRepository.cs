using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.Models;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(StoreCommerceDbContext context) : base(context)
    {
    }

    #region Queries

    public Task LockAsync(long id, CancellationToken cancellationToken)
    {
        return Context.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM cliente WHERE id = {id} FOR UPDATE", cancellationToken);
    }

    #region Get

    public async Task<(List<CustomerSearchRow> Items, int TotalCount)> SearchAsync(CustomerSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var completed = Order.CompletedStatusIds.ToList();
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Term))
        {
            var term = criteria.Term.Trim();
            var upper = term.ToUpperInvariant();
            var lower = term.ToLowerInvariant();
            query = query.Where(e => e.FullName.ToLower().Contains(lower) || e.NormalizedEmail.Contains(upper) || e.Phone.Value.Contains(term));
        }

        if (criteria.IsActive.HasValue)
            query = query.Where(e => e.IsActive == criteria.IsActive);

        if (criteria.IsVerified.HasValue)
            query = query.Where(e => e.IsVerified == criteria.IsVerified);

        if (criteria.EmailConfirmed.HasValue)
            query = query.Where(e => e.EmailConfirmed == criteria.EmailConfirmed);

        if (criteria.StateId.HasValue)
            query = query.Where(e => e.StateId == criteria.StateId);

        if (criteria.CreatedFrom.HasValue)
            query = query.Where(e => e.CreatedAt >= criteria.CreatedFrom);

        if (criteria.CreatedTo.HasValue)
            query = query.Where(e => e.CreatedAt <= criteria.CreatedTo);

        var rows = query.Select(e => new
        {
            Customer = e,
            Email = e.Email.Value,
            Phone = e.Phone.Value,
            CompletedOrders = Context.Orders.Count(order => order.CustomerId == e.Id && completed.Contains(order.OrderStatusId)),
            TotalSpent = Context.Orders.Where(order => order.CustomerId == e.Id && completed.Contains(order.OrderStatusId)).Sum(order => (decimal?)order.TotalAmount) ?? 0,
            LastOrderAt = Context.Orders.Where(order => order.CustomerId == e.Id && completed.Contains(order.OrderStatusId)).Max(order => (DateTime?)order.CreatedAt)
        });

        if (criteria.HasOrders.HasValue)
            rows = criteria.HasOrders.Value ? rows.Where(row => row.CompletedOrders > 0) : rows.Where(row => row.CompletedOrders == 0);

        var count = await rows.CountAsync(cancellationToken);
        var ordered = criteria.Sort switch
        {
            "oldest" => rows.OrderBy(row => row.Customer.CreatedAt).ThenBy(row => row.Customer.Id),
            "name" => rows.OrderBy(row => row.Customer.FullName).ThenBy(row => row.Customer.Id),
            "total_spent_desc" => rows.OrderByDescending(row => row.TotalSpent).ThenByDescending(row => row.Customer.Id),
            "last_order_desc" => rows.OrderByDescending(row => row.LastOrderAt.HasValue).ThenByDescending(row => row.LastOrderAt).ThenByDescending(row => row.Customer.Id),
            _ => rows.OrderByDescending(row => row.Customer.CreatedAt).ThenByDescending(row => row.Customer.Id)
        };

        var items = await ordered
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(row => new CustomerSearchRow(row.Customer.Id, row.Customer.FullName, row.Email, row.Phone, row.Customer.City, row.Customer.StateId,
                row.Customer.IsActive, row.Customer.IsVerified, row.Customer.EmailConfirmed, row.CompletedOrders, row.TotalSpent, row.LastOrderAt,
                row.Customer.CreatedAt))
            .ToListAsync(cancellationToken);

        return (items, count);
    }

    public async Task<IEnumerable<Customer>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.IsActive == true).ToListAsync(cancellationToken);
    }

    #endregion

    #endregion
}
