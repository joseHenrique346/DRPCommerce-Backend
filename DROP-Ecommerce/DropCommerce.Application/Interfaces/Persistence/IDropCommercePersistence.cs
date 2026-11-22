using DropCommerce.Application.Contracts;
using DropCommerce.Domain.Interfaces.Persistence;
using SharedCommerce.Application.Contracts.Drop;

namespace DropCommerce.Application.Interfaces.Persistence;

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
