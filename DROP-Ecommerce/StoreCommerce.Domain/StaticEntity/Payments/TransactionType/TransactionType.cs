namespace StoreCommerce.Domain.StaticEntity;

public sealed class TransactionType : BaseStaticEntity
{
    public static readonly TransactionType Payment = new(1, "Pagamento");
    public static readonly TransactionType Refund = new(2, "Reembolso");
    public static readonly TransactionType PartialRefund = new(3, "Reembolso parcial");
    public static readonly TransactionType Chargeback = new(4, "Estorno");

    private TransactionType(long id, string description) : base(id, description) { }
}
