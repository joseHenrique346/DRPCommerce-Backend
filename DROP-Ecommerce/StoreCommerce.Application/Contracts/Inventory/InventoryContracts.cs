using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

public sealed record AdjustStockRequest(int QuantityChange, string Reason, string IdempotencyKey, long? ExpectedVersion = null);
public sealed record StockReservationItem(long ProductVariantId, int Quantity);
public sealed record ReserveStockRequest(string IdempotencyKey, List<StockReservationItem> Items, string Reference = "");
public sealed record StockReservationLineResponse(long ProductVariantId, int Quantity, bool AllowsBackorder);
public sealed record StockReservationResponse(long Id, string IdempotencyKey, string Reference, string Status,
    DateTime ExpiresAt, DateTime? CompletedAt, List<StockReservationLineResponse> Items)
{
    public static StockReservationResponse FromEntity(StockReservation reservation)
    {
        return new(reservation.Id, reservation.IdempotencyKey, reservation.Reference, reservation.Status.ToString(),
            reservation.ExpiresAt, reservation.CompletedAt, reservation.Lines.OrderBy(Line => Line.ProductVariantId)
                .Select(Line => new StockReservationLineResponse(Line.ProductVariantId, Line.Quantity, Line.AllowsBackorder)).ToList());
    }
}

public sealed record StockMovementResponse(long Id, long ProductVariantId, long? StockReservationId, string Type,
    string OperationKey, string SKU, int QuantityChange, int ReservedChange, int StockAfter, int ReservedAfter,
    string Reason, long? ActorUserId, DateTime OccurredAt)
{
    public static StockMovementResponse FromEntity(StockMovement movement)
    {
        return new(movement.Id, movement.ProductVariantId, movement.StockReservationId, movement.Type.ToString(),
            movement.OperationKey, movement.SKU, movement.QuantityChange, movement.ReservedChange, movement.StockAfter,
            movement.ReservedAfter, movement.Reason, movement.ActorUserId, movement.OccurredAt);
    }
}
