using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Payments;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class DropSalePaymentRepository(StoreCommerceDbContext db) : IDropSalePaymentRepository
{
    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        db.Add(entity);
    }

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }

    public Task<Customer?> FindCustomerAsync(long customerId, long tenant, CancellationToken ct)
    {
        return db.Customers.IgnoreQueryFilters().AsNoTracking().Include(Customer => Customer.State)
            .SingleOrDefaultAsync(Customer => Customer.Id == customerId && Customer.EnterpriseId == tenant && Customer.IsActive && !Customer.IsDeleted, ct);
    }

    public Task<DropSalePaymentIntent?> ReadIntentAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        return db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Intent => Intent.EnterpriseId == tenant && Intent.OperationId == operationId, ct);
    }

    public Task<DropSalePaymentIntent?> FindIntentAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        return db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().SingleOrDefaultAsync(Intent => Intent.EnterpriseId == tenant && Intent.OperationId == operationId, ct);
    }

    public Task<List<DropSalePaymentIntent>> ListDueAsync(DateTime now, CancellationToken ct)
    {
        return db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
            .Where(Intent => Intent.Status == "pending" && Intent.NextCheckAt <= now)
            .OrderBy(Intent => Intent.NextCheckAt).ThenBy(Intent => Intent.Id).Take(50).ToListAsync(ct);
    }

    public async Task LockCustomerAndTenantAsync(long tenant, long customerId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM cliente WHERE id = {customerId} AND enterprise_id = {tenant} FOR UPDATE", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
    }

    public Task LockTenantAsync(long tenant, CancellationToken ct)
    {
        return db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
    }

    public Task<List<DropSalePaymentIntent>> PendingReceiptsAsync(CancellationToken ct)
    {
        return db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().AsNoTracking()
            .Where(Intent => Intent.AcknowledgedRevision < Intent.Revision)
            .OrderBy(Intent => Intent.Id).Take(50).ToListAsync(ct);
    }
}
