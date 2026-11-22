using StoreCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class StockReservationLine : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long StockReservationId { get; private set; }
    public long ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public bool AllowsBackorder { get; private set; }
    public StockReservation StockReservation { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    protected StockReservationLine()
    {
    }

    internal static StockReservationLine Create(StockReservation reservation, ProductVariant variant, int quantity, bool allowBackorder)
    {
        return new StockReservationLine
        {
            EnterpriseId = reservation.EnterpriseId,
            StockReservation = reservation,
            StockReservationId = reservation.Id,
            ProductVariant = variant,
            ProductVariantId = variant.Id,
            Quantity = quantity,
            AllowsBackorder = allowBackorder
        };
    }
}
