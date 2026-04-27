namespace DropCommerce.Domain.Entity;

public class DropReservation : BaseEntity
{
    public long DropEventId { get; private set; }
    public long DropProductId { get; private set; }
    public long CustomerId { get; private set; }
    public long QueueEntryId { get; private set; }
    public long StatusId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string LockToken { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public DropReservation() { }

    public DropReservation(long dropEventId, long dropProductId, long customerId, long queueEntryId, long statusId, int quantity, decimal unitPrice, decimal totalAmount, string lockToken, DateTime reservedAt, DateTime expiresAt, DateTime? confirmedAt, DateTime? cancelledAt)
    {
        DropEventId = dropEventId;
        DropProductId = dropProductId;
        CustomerId = customerId;
        QueueEntryId = queueEntryId;
        StatusId = statusId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = totalAmount;
        LockToken = lockToken;
        ReservedAt = reservedAt;
        ExpiresAt = expiresAt;
        ConfirmedAt = confirmedAt;
        CancelledAt = cancelledAt;
    }
}
