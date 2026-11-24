using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

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
