namespace DropCommerce.Domain.Entity;

public class DropNotification : BaseEntity
{
    #region Properties 

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long ChannelId { get; private set; }
    public long TypeId { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public long StatusId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    #endregion

    #region Constructors

    protected DropNotification() { }

    private DropNotification(long dropEventId, long customerId, long channelId, long typeId, string subject, string body, long statusId, DateTime scheduledAt, DateTime? sentAt)
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

    #endregion

    #region Functions

    public static DropNotification Create(long dropEventId, long customerId, long channelId, long typeId, string subject, string body, long statusId, DateTime scheduledAt, DateTime? sentAt)
    {
        BaseValidate<long>.ValidateNotNullValue(dropEventId);
        BaseValidate<long>.ValidateIdValue(dropEventId);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<long>.ValidateNotNullValue(channelId);
        BaseValidate<long>.ValidateIdValue(channelId);

        BaseValidate<long>.ValidateNotNullValue(typeId);
        BaseValidate<long>.ValidateIdValue(typeId);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(subject);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(body);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<DateTime>.ValidateNotNullValue(scheduledAt);

        return new DropNotification(dropEventId, customerId, channelId, typeId, subject, body, statusId, scheduledAt, sentAt);
    }

    #endregion
}