namespace StoreCommerce.Domain.StaticEntity;

public sealed class OrderPaymentStatus : BaseStaticEntity
{
    public static readonly OrderPaymentStatus Pending = new(1, "Pendente");
    public static readonly OrderPaymentStatus Paid = new(2, "Pago");
    public static readonly OrderPaymentStatus PartialRefund = new(3, "Reembolso parcial");
    public static readonly OrderPaymentStatus FullRefund = new(4, "Reembolso total");
    public static readonly OrderPaymentStatus Failed = new(5, "Falhou");

    private OrderPaymentStatus(long id, string description) : base(id, description) { }
}
