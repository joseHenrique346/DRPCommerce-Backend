namespace DropCommerce.Domain.Entity;

public class DropTransaction : BaseEntity
{
    #region Properties

    public long DropOrderId { get; private set; }
    public long CustomerId { get; private set; }
    public long TypeId { get; private set; }
    public long MethodId { get; private set; }
    public long StatusId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Fee { get; private set; }
    public string GatewayReference { get; private set; }
    public string GatewayProvider { get; private set; }
    public string GatewayPayload { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    #endregion

    #region Constructors

    protected DropTransaction() { }

    private DropTransaction(long dropOrderId, long customerId, long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        DropOrderId = dropOrderId;
        CustomerId = customerId;
        TypeId = typeId;
        MethodId = methodId;
        StatusId = statusId;
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

    public static DropTransaction Create(long dropOrderId, long customerId, long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        BaseValidate<long>.ValidateNotNullValue(dropOrderId);
        BaseValidate<long>.ValidateIdValue(dropOrderId);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<long>.ValidateNotNullValue(typeId);
        BaseValidate<long>.ValidateIdValue(typeId);

        BaseValidate<long>.ValidateNotNullValue(methodId);
        BaseValidate<long>.ValidateIdValue(methodId);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<decimal>.ValidateNotNullValue(amount);
        BaseValidate<decimal>.ValidateNotNullValue(fee);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(gatewayReference);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(gatewayProvider);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(gatewayPayload);

        return new DropTransaction(dropOrderId, customerId, typeId, methodId, statusId, amount, fee, gatewayReference, gatewayProvider, gatewayPayload, paidAt, refundedAt);
    }

    #endregion
}