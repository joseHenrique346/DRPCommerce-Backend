namespace DropCommerce.Domain.StaticEntity;

public sealed class DropOrderStatus : BaseStaticEntity
{
    public static readonly DropOrderStatus Pending = new(1, "Pendente");
    public static readonly DropOrderStatus Confirmed = new(2, "Confirmado");
    public static readonly DropOrderStatus Processing = new(3, "Em processamento");
    public static readonly DropOrderStatus Shipped = new(4, "Enviado");
    public static readonly DropOrderStatus Delivered = new(5, "Entregue");
    public static readonly DropOrderStatus Cancelled = new(6, "Cancelado");
    public static readonly DropOrderStatus Refunded = new(7, "Reembolsado");

    private DropOrderStatus(long id, string description) : base(id, description) { }
}