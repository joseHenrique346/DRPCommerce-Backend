namespace DropCommerce.Domain.Entity;

public class DropNotification : BaseEntity
{
    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long ChannelId { get; private set; }
    public long TypeId { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public long StatusId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    public DropNotification() { }

    public DropNotification(long dropEventId, long customerId, long channelId, long typeId, string subject, string body, long statusId, DateTime scheduledAt, DateTime? sentAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        ChannelId = channelId;
        TypeId = typeId;
        Subject = subject;
        Body = body;
        StatusId = statusId;
        ScheduledAt = scheduledAt;
        SentAt = sentAt;
    }
}
