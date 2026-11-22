using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Contracts;

public interface IDropStockSagaRepository : IDropCommercePersistence
{
    Task<DropStockImportReceipt?> FindReceiptAsync(Guid operationId, CancellationToken ct);
    Task<DropCatalogProduct?> FindSourceProductAsync(long tenant, long productId, CancellationToken ct);
    Task<int> CountCatalogAsync(long tenant, CancellationToken ct);
    Task<DropStockReturnIntent?> FindPendingReturnAsync(long tenant, long productId, CancellationToken ct);
    Task<DropCatalogProduct?> FindProductWithVariantsAsync(long productId, CancellationToken ct);
    Task<bool> HasFutureEventsAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasReservationsAsync(long productId, long tenant, CancellationToken ct);
    Task<bool> HasPendingOrdersAsync(long productId, long tenant, CancellationToken ct);
    Task<DropStockReturnIntent> GetReturnAsync(long tenant, Guid operationId, CancellationToken ct);
    Task<DropCatalogProduct> GetProductAsync(long productId, long tenant, CancellationToken ct);
}
