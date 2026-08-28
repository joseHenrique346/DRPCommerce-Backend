namespace StoreCommerce.Domain.StaticEntity;

public sealed class ShipmentType : BaseStaticEntity
{
    public static readonly ShipmentType Standard = new(1, "Normal");
    public static readonly ShipmentType Express = new(2, "Expresso");
    public static readonly ShipmentType Economy = new(3, "Econômico");
    public static readonly ShipmentType Pickup = new(4, "Retirada no local");

    private ShipmentType(long id, string description) : base(id, description) { }
}
