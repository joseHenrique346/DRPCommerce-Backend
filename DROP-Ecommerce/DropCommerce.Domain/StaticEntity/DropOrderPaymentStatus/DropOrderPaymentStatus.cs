namespace DropCommerce.Domain.StaticEntity;

public sealed class DropOrderPaymentStatus : BaseStaticEntity
{
    public static readonly DropOrderPaymentStatus Pending = new(1, "Pendente");
    public static readonly DropOrderPaymentStatus Paid = new(2, "Pago");
    public static readonly DropOrderPaymentStatus PartialRefund = new(3, "Reembolso parcial");
    public static readonly DropOrderPaymentStatus FullRefund = new(4, "Reembolso total");
    public static readonly DropOrderPaymentStatus Failed = new(5, "Falhou");

    private DropOrderPaymentStatus(long id, string description) : base(id, description) { }
}