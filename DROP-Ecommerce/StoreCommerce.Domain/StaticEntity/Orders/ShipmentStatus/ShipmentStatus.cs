namespace StoreCommerce.Domain.StaticEntity;

public sealed class ShipmentStatus : BaseStaticEntity
{
    public static readonly ShipmentStatus Pending = new(1, "Pendente");
    public static readonly ShipmentStatus Processing = new(2, "Em processamento");
    public static readonly ShipmentStatus Shipped = new(3, "Enviado");
    public static readonly ShipmentStatus InTransit = new(4, "Em trânsito");
    public static readonly ShipmentStatus Delivered = new(5, "Entregue");
    public static readonly ShipmentStatus Cancelled = new(6, "Cancelado");
    public static readonly ShipmentStatus Returned = new(7, "Devolvido");

    private ShipmentStatus(long id, string description) : base(id, description) { }
}
