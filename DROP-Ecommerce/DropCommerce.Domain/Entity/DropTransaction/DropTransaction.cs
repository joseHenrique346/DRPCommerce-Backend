namespace DropCommerce.Domain.Entity;

public class DropTransaction : BaseEntity
{
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

    public DropTransaction() { }

    public DropTransaction(long dropOrderId, long customerId, long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
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
}
