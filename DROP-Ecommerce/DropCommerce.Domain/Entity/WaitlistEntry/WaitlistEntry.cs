namespace DropCommerce.Domain.Entity;

public class WaitlistEntry : BaseEntity
{
    public long DropEventId { get; private set; }
    public long? DropProductId { get; private set; }
    public long CustomerId { get; private set; }
    public int Position { get; private set; }
    public long StatusId { get; private set; }
    public bool NotificationSent { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public WaitlistEntry() { }

    public WaitlistEntry(long dropEventId, long? dropProductId, long customerId, int position, long statusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt)
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
}
