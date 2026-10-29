using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<List<ProductVariant>> GetVariantsAsync(long tenant, IReadOnlyCollection<long> ids, CancellationToken ct);
    Task<StockReservation?> FindByKeyAsync(long tenant, string key, CancellationToken ct);
    Task<StockReservation?> GetReservationAsync(long tenant, long id, CancellationToken ct, bool tracking = true);
    Task<List<StockReservation>> GetExpiredAsync(long tenant, DateTime now, CancellationToken ct);
    Task<List<long>> GetExpiredTenantsAsync(DateTime now, CancellationToken ct);
    Task<StockMovement?> GetAdjustmentAsync(long tenant, long variantId, string key, CancellationToken ct);
    Task<(List<StockMovement> Items, int TotalCount)> GetMovementsAsync(long tenant, long? variantId, int page, int pageSize, CancellationToken ct);
    void AddReservation(StockReservation reservation);
    void AddMovement(StockMovement movement);
}
