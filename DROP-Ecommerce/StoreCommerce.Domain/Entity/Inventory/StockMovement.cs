using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public enum StockMovementType
{
    Adjustment = 1,
    Reservation = 2,
    Release = 3,
    Commit = 4,
    Expiration = 5
}

public class StockMovement : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long ProductVariantId { get; private set; }
    public long? StockReservationId { get; private set; }
    public StockMovementType Type { get; private set; }
    public string OperationKey { get; private set; } = string.Empty;
    public int QuantityChange { get; private set; }
    public int ReservedChange { get; private set; }
    public int StockAfter { get; private set; }
    public int ReservedAfter { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public long? ActorUserId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;
    public StockReservation? StockReservation { get; private set; }

    protected StockMovement()
    {
    }

    public static StockMovement Create(ProductVariant variant, StockReservation? reservation, StockMovementType type,
        string key, int quantityChange, int reservedChange, string reason, long? actorUserId, DateTime now)
    {
        return new StockMovement
        {
            EnterpriseId = variant.EnterpriseId,
            ProductVariant = variant,
            ProductVariantId = variant.Id,
            StockReservation = reservation,
            StockReservationId = reservation?.Id,
            Type = type,
            OperationKey = key,
            QuantityChange = quantityChange,
            ReservedChange = reservedChange,
            StockAfter = variant.StockQuantity,
            ReservedAfter = variant.ReservedQuantity,
            Reason = reason,
            SKU = variant.SKU,
            ActorUserId = actorUserId,
            OccurredAt = now
        };
    }
}
