namespace DropCommerce.Domain.Entity;

public class WaitlistEntry : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long? DropProductId { get; private set; }
    public long CustomerId { get; private set; }
    public int Position { get; private set; }
    public long StatusId { get; private set; }
    public bool NotificationSent { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    #endregion

    #region Constructors

    protected WaitlistEntry() { }

    private WaitlistEntry(long dropEventId, long? dropProductId, long customerId, int position, long statusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt)
    {
        DropEventId = dropEventId;
        DropProductId = dropProductId;
        CustomerId = customerId;
        Position = position;
        StatusId = statusId;
        NotificationSent = notificationSent;
        JoinedAt = joinedAt;
        NotifiedAt = notifiedAt;
        ExpiresAt = expiresAt;
    }

    #endregion

    #region Functions

    public static WaitlistEntry Create(long dropEventId, long? dropProductId, long customerId, int position, long statusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt)
    {
        BaseValidate<long>.ValidateNotNullValue(dropEventId);
        BaseValidate<long>.ValidateIdValue(dropEventId);

        BaseValidate<long>.ValidateIdValue(dropProductId ?? 1);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<int>.ValidateNotNullValue(position);

        BaseValidate<bool>.ValidateNotNullValue(notificationSent);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<DateTime>.ValidateNotNullValue(joinedAt);
        BaseValidate<DateTime>.ValidateNotNullValue(expiresAt);

        return new WaitlistEntry(dropEventId, dropProductId, customerId, position, statusId, notificationSent, joinedAt, notifiedAt, expiresAt);
    }

    #endregion
}