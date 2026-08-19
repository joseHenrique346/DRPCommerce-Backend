using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Transaction : BaseEntity
{
    #region Properties
    public long OrderId { get; private set; }
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

    #region Constructor
    protected Transaction() { }

    private Transaction(long orderId, long customerId, long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        OrderId = orderId;
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
    public static Transaction Create(long orderId, long customerId, long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
    {
        BaseValidate.ValidatePositive(orderId, nameof(OrderId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidatePositive(methodId, nameof(MethodId));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositive(amount, nameof(Amount));
        BaseValidate.ValidatePositiveOrZero(fee, nameof(Fee));
        BaseValidate.ValidateMaxLength(gatewayReference, 255, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayProvider, 255, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayPayload, 5000, nameof(GatewayPayload));
        BaseValidate.ValidateNullableNotFuture(paidAt, nameof(PaidAt));
        BaseValidate.ValidateNullableNotFuture(refundedAt, nameof(RefundedAt));

        return new Transaction(orderId, customerId, typeId, methodId, statusId, amount, fee, gatewayReference, gatewayProvider, gatewayPayload, paidAt, refundedAt);
    }

    public void UpdateDetails(long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload)
    {
        BaseValidate.ValidatePositive(typeId, nameof(TypeId));
        BaseValidate.ValidatePositive(methodId, nameof(MethodId));
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));
        BaseValidate.ValidatePositive(amount, nameof(Amount));
        BaseValidate.ValidatePositiveOrZero(fee, nameof(Fee));
        BaseValidate.ValidateMaxLength(gatewayReference, 255, nameof(GatewayReference));
        BaseValidate.ValidateMaxLength(gatewayProvider, 255, nameof(GatewayProvider));
        BaseValidate.ValidateMaxLength(gatewayPayload, 5000, nameof(GatewayPayload));

        TypeId = typeId;
        MethodId = methodId;
        StatusId = statusId;
        Amount = amount;
        Fee = fee;
        GatewayReference = gatewayReference;
        GatewayProvider = gatewayProvider;
        GatewayPayload = gatewayPayload;
    }

    public void UpdateStatus(long statusId)
    {
        BaseValidate.ValidatePositive(statusId, nameof(StatusId));

        StatusId = statusId;
    }
    #endregion
}
