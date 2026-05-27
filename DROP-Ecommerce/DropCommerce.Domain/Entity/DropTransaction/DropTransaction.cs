using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class DropTransaction : BaseEntity
{
    #region Properties

    public long DropOrderId { get; private set; }
    public long CustomerId { get; private set; }
    public long DropTransactionTypeId { get; private set; }
    public long DropTransactionMethodId { get; private set; }
    public long DropTransactionStatusId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Fee { get; private set; }
    public string GatewayReference { get; private set; }
    public string GatewayProvider { get; private set; }
    public string GatewayPayload { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    #region Navigation Properties

    public DropOrder DropOrder { get; private set; }
    public DropTransactionType DropTransactionType { get; private set; }
    public DropTransactionMethod DropTransactionMethod { get; private set; }
    public DropTransactionStatus DropTransactionStatus { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected DropTransaction() { }

    private DropTransaction(long dropOrderId, long customerId, long dropTransactionTypeId, long dropTransactionMethodId, long dropTransactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        DropOrderId = dropOrderId;
        CustomerId = customerId;
        DropTransactionTypeId = dropTransactionTypeId;
        DropTransactionMethodId = dropTransactionMethodId;
        DropTransactionStatusId = dropTransactionStatusId;
        Amount = amount;
        Fee = fee;
        GatewayReference = gatewayReference;
        GatewayProvider = gatewayProvider;
        GatewayPayload = gatewayPayload;
        PaidAt = paidAt;
        RefundedAt = refundedAt;
    }

    #endregion

    #region Functions

    public static DropTransaction Create(long dropOrderId, long customerId, long dropTransactionTypeId, long dropTransactionMethodId, long dropTransactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        BaseValidate.ValidateId(dropOrderId, nameof(dropOrderId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropTransactionTypeId, nameof(dropTransactionTypeId));
        BaseValidate.ValidateId(dropTransactionMethodId, nameof(dropTransactionMethodId));
        BaseValidate.ValidateId(dropTransactionStatusId, nameof(dropTransactionStatusId));
        BaseValidate.ValidateMinimumDecimal(amount, 0.01m, nameof(amount));
        BaseValidate.ValidatePositiveDecimal(fee, nameof(fee));
        BaseValidate.ValidateString(gatewayReference, nameof(gatewayReference));
        BaseValidate.ValidateString(gatewayProvider, nameof(gatewayProvider));
        BaseValidate.ValidateString(gatewayPayload, nameof(gatewayPayload));

        return new DropTransaction(dropOrderId, customerId, dropTransactionTypeId, dropTransactionMethodId, dropTransactionStatusId, amount, fee, gatewayReference, gatewayProvider, gatewayPayload, paidAt, refundedAt);
    }

    #endregion
}
