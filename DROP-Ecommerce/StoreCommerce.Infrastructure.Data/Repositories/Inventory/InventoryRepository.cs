using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class InventoryRepository(StoreCommerceDbContext db) : IInventoryRepository
{
    private IQueryable<StockReservation> Reservations(long tenant, bool tracking = true)
    {
        var query = db.StockReservations.IgnoreQueryFilters()
            .Include(Reservation => Reservation.Lines).ThenInclude(Line => Line.ProductVariant)
            .Where(Reservation => Reservation.EnterpriseId == tenant);

        return tracking ? query : query.AsNoTracking();
    }

    public Task<List<ProductVariant>> GetVariantsAsync(long tenant, IReadOnlyCollection<long> ids, CancellationToken ct)
    {
        return db.ProductVariants.IgnoreQueryFilters().Include(Variant => Variant.Product)
            .Where(Variant => Variant.EnterpriseId == tenant && ids.Contains(Variant.Id) && !Variant.IsDeleted && !Variant.Product.IsDeleted)
            .ToListAsync(ct);
    }

    public Task<StockReservation?> FindByKeyAsync(long tenant, string key, CancellationToken ct)
    {
        return Reservations(tenant).SingleOrDefaultAsync(Reservation => Reservation.IdempotencyKey == key, ct);
    }

    public Task<StockReservation?> GetReservationAsync(long tenant, long id, CancellationToken ct, bool tracking = true)
    {
        return Reservations(tenant, tracking).SingleOrDefaultAsync(Reservation => Reservation.Id == id, ct);
    }

    public Task<bool> IsAttachedToOrderAsync(long tenant, long reservationId, CancellationToken ct)
    {
        // O vínculo continua protegido mesmo se o pedido tiver sido excluído logicamente.
        return db.Orders.IgnoreQueryFilters().AnyAsync(Order =>
            Order.EnterpriseId == tenant && Order.StockReservationId == reservationId, ct);
    }

    private IQueryable<StockReservation> ExpiredStandaloneReservations(DateTime now)
    {
        return db.StockReservations.IgnoreQueryFilters().Where(Reservation =>
            Reservation.Status == StockReservationStatus.Reserved && Reservation.ExpiresAt <= now &&
            !db.Orders.IgnoreQueryFilters().Any(Order =>
                Order.EnterpriseId == Reservation.EnterpriseId && Order.StockReservationId == Reservation.Id));
    }

    public Task<List<StockReservation>> GetExpiredAsync(long tenant, DateTime now, CancellationToken ct)
    {
        return ExpiredStandaloneReservations(now)
            .Where(Reservation => Reservation.EnterpriseId == tenant)
            .Include(Reservation => Reservation.Lines).ThenInclude(Line => Line.ProductVariant)
            .ToListAsync(ct);
    }

    public Task<List<long>> GetExpiredTenantsAsync(DateTime now, CancellationToken ct)
    {
        return ExpiredStandaloneReservations(now).AsNoTracking()
            .Select(Reservation => Reservation.EnterpriseId).Distinct().OrderBy(Tenant => Tenant).Take(200).ToListAsync(ct);
    }

    public Task<StockMovement?> GetAdjustmentAsync(long tenant, long variantId, string key, CancellationToken ct)
    {
        return db.StockMovements.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(Movement =>
            Movement.EnterpriseId == tenant && Movement.ProductVariantId == variantId &&
            Movement.Type == StockMovementType.Adjustment && Movement.OperationKey == key, ct);
    }

    public async Task<(List<StockMovement> Items, int TotalCount)> GetMovementsAsync(long tenant, long? variantId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.StockMovements.IgnoreQueryFilters().AsNoTracking().Where(Movement => Movement.EnterpriseId == tenant);

        if (variantId.HasValue)
            query = query.Where(Movement => Movement.ProductVariantId == variantId);

        var count = await query.CountAsync(ct);
        var items = await query.OrderByDescending(Movement => Movement.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, count);
    }

    public void AddReservation(StockReservation reservation)
    {
        db.StockReservations.Add(reservation);
    }

    public void AddMovement(StockMovement movement)
    {
        db.StockMovements.Add(movement);
    }
}
