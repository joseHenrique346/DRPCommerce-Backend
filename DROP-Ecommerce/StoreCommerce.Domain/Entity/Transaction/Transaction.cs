using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Transaction : BaseEntity
{
    #region Properties
    public long OrderId { get; private set; }
    public long CustomerId { get; private set; }
    public long TransactionTypeId { get; private set; }
    public long TransactionMethodId { get; private set; }
    public long TransactionStatusId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Fee { get; private set; }
    public string GatewayReference { get; private set; }
    public string GatewayProvider { get; private set; }
    public string GatewayPayload { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    #endregion

    #region Constructor
    protected Transaction() { }

    private Transaction(long orderId, long customerId, long transactionTypeId, long transactionMethodId, long transactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TransactionTypeId = transactionTypeId;
        TransactionMethodId = transactionMethodId;
        TransactionStatusId = transactionStatusId;
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
    public static Transaction Create(long orderId, long customerId, long transactionTypeId, long transactionMethodId, long transactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        BaseValidate.ValidatePositive(transactionTypeId, nameof(TransactionTypeId));
        BaseValidate.ValidatePositive(transactionMethodId, nameof(TransactionMethodId));
        BaseValidate.ValidatePositive(transactionStatusId, nameof(TransactionStatusId));
        BaseValidate.ValidatePositive(amount, nameof(Amount));
        BaseValidate.ValidatePositiveOrZero(fee, nameof(Fee));
        BaseValidate.ValidateMaxLength(gatewayReference, 255, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayProvider, 255, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayPayload, 5000, nameof(GatewayPayload));
        BaseValidate.ValidateNullableNotFuture(paidAt, nameof(PaidAt));
        BaseValidate.ValidateNullableNotFuture(refundedAt, nameof(RefundedAt));

        return new Transaction(orderId, customerId, transactionTypeId, transactionMethodId, transactionStatusId, amount, fee, gatewayReference, gatewayProvider, gatewayPayload, paidAt, refundedAt);
    }

    public void UpdateDetails(long transactionTypeId, long transactionMethodId, long transactionStatusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload)
    {
        BaseValidate.ValidatePositive(transactionTypeId, nameof(TransactionTypeId));
        BaseValidate.ValidatePositive(transactionMethodId, nameof(TransactionMethodId));
        BaseValidate.ValidatePositive(transactionStatusId, nameof(TransactionStatusId));
        BaseValidate.ValidatePositive(amount, nameof(Amount));
        BaseValidate.ValidatePositiveOrZero(fee, nameof(Fee));
        BaseValidate.ValidateMaxLength(gatewayReference, 255, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayProvider, 255, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayPayload, 5000, nameof(GatewayPayload));

        TransactionTypeId = transactionTypeId;
        TransactionMethodId = transactionMethodId;
        TransactionStatusId = transactionStatusId;
        Amount = amount;
        Fee = fee;
        GatewayReference = gatewayReference;
        GatewayProvider = gatewayProvider;
        GatewayPayload = gatewayPayload;
    }

    public void UpdateStatus(long transactionStatusId)
    {
        BaseValidate.ValidatePositive(transactionStatusId, nameof(TransactionStatusId));

        TransactionStatusId = transactionStatusId;
    }
    #endregion
}
