namespace DropCommerce.Domain.Entity;

public class DropReservation : BaseEntity
{
    #region Properties

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

    #endregion

    #region Constructors

    protected DropReservation() { }

    private DropReservation(long dropEventId, long dropProductId, long customerId, long queueEntryId, long statusId, int quantity, decimal unitPrice, decimal totalAmount, string lockToken, DateTime reservedAt, DateTime expiresAt, DateTime? confirmedAt, DateTime? cancelledAt)
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

    #endregion

    #region Functions

    public static DropReservation Create(long dropEventId, long dropProductId, long customerId, long queueEntryId, long statusId, int quantity, decimal unitPrice, decimal totalAmount, string lockToken, DateTime reservedAt, DateTime expiresAt, DateTime? confirmedAt, DateTime? cancelledAt)
    {
        BaseValidate<long>.ValidateNotNullValue(dropEventId);
        BaseValidate<long>.ValidateIdValue(dropEventId);

        BaseValidate<long>.ValidateNotNullValue(dropProductId);
        BaseValidate<long>.ValidateIdValue(dropProductId);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<long>.ValidateNotNullValue(queueEntryId);
        BaseValidate<long>.ValidateIdValue(queueEntryId);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<int>.ValidateNotNullValue(quantity);

        BaseValidate<decimal>.ValidateNotNullValue(unitPrice);
        BaseValidate<decimal>.ValidateNotNullValue(totalAmount);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(lockToken);

        BaseValidate<DateTime>.ValidateNotNullValue(reservedAt);
        BaseValidate<DateTime>.ValidateNotNullValue(expiresAt);

        return new DropReservation(dropEventId, dropProductId, customerId, queueEntryId, statusId, quantity, unitPrice, totalAmount, lockToken, reservedAt, expiresAt, confirmedAt, cancelledAt);
    }


    #endregion
}