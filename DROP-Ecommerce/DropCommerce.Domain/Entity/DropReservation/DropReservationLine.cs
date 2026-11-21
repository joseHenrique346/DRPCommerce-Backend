namespace DropCommerce.Domain.Entity;

public sealed class DropReservationLine : BaseEntity
{
    public long ReservationId { get; private set; }
    public DropReservation Reservation { get; private set; } = null!;
    public long VariantId { get; private set; }
    public DropCatalogVariant Variant { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public string OptionsJson { get; private set; } = "{}";

    private DropReservationLine()
    {
    }

    public static DropReservationLine Capture(DropReservation reservation, DropCatalogVariant variant, int quantity)
    {
        return new DropReservationLine
        {
            Reservation = reservation, ReservationId = reservation.Id, Variant = variant, VariantId = variant.Id,
            Quantity = quantity, UnitPrice = variant.Price, SKU = variant.SKU,
            ItemName = variant.CatalogProduct.Name, OptionsJson = variant.OptionsJson
        };
    }
}
