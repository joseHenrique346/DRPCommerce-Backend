namespace DropCommerce.Domain.StaticEntity;

public sealed class DropTransactionType : BaseStaticEntity
{
    public static readonly DropTransactionType Payment = new(1, "Pagamento");
    public static readonly DropTransactionType Refund = new(2, "Reembolso");
    public static readonly DropTransactionType PartialRefund = new(3, "Reembolso parcial");
    public static readonly DropTransactionType Chargeback = new(4, "Chargeback");

    private DropTransactionType(long id, string description) : base(id, description) { }
}