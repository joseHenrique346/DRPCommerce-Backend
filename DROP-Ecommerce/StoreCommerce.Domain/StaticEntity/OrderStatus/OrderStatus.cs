namespace StoreCommerce.Domain.StaticEntity;

public sealed class OrderStatus : BaseStaticEntity
{
    public static readonly OrderStatus Pending = new(1, "Pendente");
    public static readonly OrderStatus Confirmed = new(2, "Confirmado");
    public static readonly OrderStatus Processing = new(3, "Em processamento");
    public static readonly OrderStatus Shipped = new(4, "Enviado");
    public static readonly OrderStatus Delivered = new(5, "Entregue");
    public static readonly OrderStatus Cancelled = new(6, "Cancelado");
    public static readonly OrderStatus Refunded = new(7, "Reembolsado");

    private OrderStatus(long id, string description) : base(id, description) { }
}
