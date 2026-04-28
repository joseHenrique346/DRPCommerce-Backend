namespace StoreCommerce.Domain.Entity;

public class Transaction : BaseEntity
{
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

    public Transaction() { }

    public Transaction(long orderId, long customerId, long typeId, long methodId, long statusId, decimal amount, decimal fee, string gatewayReference, string gatewayProvider, string gatewayPayload, DateTime? paidAt, DateTime? refundedAt)
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
}
