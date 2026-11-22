using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Contracts.Drop;

namespace DropCommerce.Application.Contracts;

public interface IDropCommercePersistence
{
    Task<IDropTransaction> BeginTransactionAsync(CancellationToken ct);
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void ClearTracking();
    Task LockCommercialTenantAsync(long tenant, CancellationToken ct);
    Task<DropCommercialRights> RefreshCommercialRightsAsync(long tenant, CancellationToken ct);
    Task ApplyCommercialRightsAsync(DropCommercialRights rights, CancellationToken ct);
    void RecordCommercialActivity(DropCommercialRights rights, string action, long? catalogId, long? participationId);
}
