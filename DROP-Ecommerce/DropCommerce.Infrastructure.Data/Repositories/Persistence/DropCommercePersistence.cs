using DropCommerce.Application.Interfaces.Persistence;
using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Contracts;
using DropCommerce.Infrastructure.Data.Context;
using SharedCommerce.Application.Contracts.Drop;

namespace DropCommerce.Infrastructure.Data.Repositories;

public abstract class DropCommercePersistence(DropCommerceDbContext db) : IDropCommercePersistence
{
    protected DropCommerceDbContext Context { get; } = db;

    public async Task<IDropTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        return new DropCommerceTransaction(await Context.Database.BeginTransactionAsync(ct));
    }

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        Context.Add(entity);
    }

    public void ClearTracking()
    {
        Context.ChangeTracker.Clear();
    }

    public Task LockCommercialTenantAsync(long tenant, CancellationToken ct)
    {
        return Context.LockCommercialTenantAsync(tenant, ct);
    }

    public Task<DropCommercialRights> RefreshCommercialRightsAsync(long tenant, CancellationToken ct)
    {
        return Context.RefreshCommercialRightsAsync(tenant, ct);
    }

    public Task ApplyCommercialRightsAsync(DropCommercialRights rights, CancellationToken ct)
    {
        return Context.ApplyCommercialRightsAsync(rights, ct);
    }

    public void RecordCommercialActivity(DropCommercialRights rights, string action, long? catalogId, long? participationId)
    {
        Context.RecordCommercialActivity(rights, action, catalogId, participationId);
    }
}
