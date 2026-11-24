using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;
using Product = StoreCommerce.Domain.Entity.Product;

namespace StoreCommerce.Application.Interfaces.Inventory;

public interface IDropStockTransferRepository
{
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void ClearTracking();
    Task<DropStockTransfer?> FindTransferByKeyAsync(long tenant, string key, CancellationToken ct);
    Task<Enterprise> GetEnterpriseAsync(long tenant, CancellationToken ct);
    Task<StoreCommerce.Domain.Entity.Product?> FindProductAsync(long productId, long tenant, CancellationToken ct);
    Task<List<ProductVariant>> ListVariantsAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasReservationsAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasOpenOrdersAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasUncertainPaymentsAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasPaymentOperationsAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasPendingPaymentsAsync(long productId, long tenant, CancellationToken ct);
    Task<DropStockTransfer> ReadTransferAsync(Guid operationId, long tenant, CancellationToken ct);
    Task<DropStockTransfer> GetTransferAsync(long transferId, CancellationToken ct);
    Task<DropStockTransfer?> FindOriginalExportAsync(Guid exportId, long tenant, long productId, CancellationToken ct);
    Task<StoreCommerce.Domain.Entity.Product> GetProductAsync(long productId, long tenant, CancellationToken ct);
    Task<ProductVariant> GetVariantAsync(long variantId, long productId, long tenant, CancellationToken ct);
    Task LockStockAsync(long tenant, long productId, CancellationToken ct);
    Task<List<string>> PendingSnapshotsAsync(CancellationToken ct);
}
