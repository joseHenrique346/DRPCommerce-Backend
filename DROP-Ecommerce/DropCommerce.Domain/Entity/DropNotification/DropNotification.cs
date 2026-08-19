using DropCommerce.Domain.StaticEntity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Domain.Entity;

public class DropNotification : BaseEntity, ISoftDeletable
{
    #region Properties

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long DropNotificationChannelId { get; private set; }
    public long DropNotificationTypeId { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public long DropNotificationStatusId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public DropNotificationChannel DropNotificationChannel { get; private set; }
    public DropNotificationType DropNotificationType { get; private set; }
    public DropNotificationStatus DropNotificationStatus { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected DropNotification() { }

    private DropNotification(long dropEventId, long customerId, long dropNotificationChannelId, long dropNotificationTypeId, string subject, string body, long dropNotificationStatusId, DateTime scheduledAt, DateTime? sentAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        DropNotificationChannelId = dropNotificationChannelId;
        DropNotificationTypeId = dropNotificationTypeId;
        Subject = subject;
        Body = body;
        DropNotificationStatusId = dropNotificationStatusId;
        ScheduledAt = scheduledAt;
        SentAt = sentAt;
    }

    #endregion

    #region Functions

    public static DropNotification Create(long dropEventId, long customerId, long dropNotificationChannelId, long dropNotificationTypeId, string subject, string body, long dropNotificationStatusId, DateTime scheduledAt, DateTime? sentAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropNotificationChannelId, nameof(dropNotificationChannelId));
        BaseValidate.ValidateId(dropNotificationTypeId, nameof(dropNotificationTypeId));
        BaseValidate.ValidateString(subject, nameof(subject));
        BaseValidate.ValidateString(body, nameof(body));
        BaseValidate.ValidateId(dropNotificationStatusId, nameof(dropNotificationStatusId));
        BaseValidate.ValidateDate(scheduledAt, nameof(scheduledAt));

        return new DropNotification(dropEventId, customerId, dropNotificationChannelId, dropNotificationTypeId, subject, body, dropNotificationStatusId, scheduledAt, sentAt);
    }

    public void Update(long dropEventId, long customerId, long dropNotificationChannelId, long dropNotificationTypeId, string subject, string body, long dropNotificationStatusId, DateTime scheduledAt, DateTime? sentAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropNotificationChannelId, nameof(dropNotificationChannelId));
        BaseValidate.ValidateId(dropNotificationTypeId, nameof(dropNotificationTypeId));
        BaseValidate.ValidateString(subject, nameof(subject));
        BaseValidate.ValidateString(body, nameof(body));
        BaseValidate.ValidateId(dropNotificationStatusId, nameof(dropNotificationStatusId));
        BaseValidate.ValidateDate(scheduledAt, nameof(scheduledAt));

        DropEventId = dropEventId;
        CustomerId = customerId;
        DropNotificationChannelId = dropNotificationChannelId;
        DropNotificationTypeId = dropNotificationTypeId;
        Subject = subject;
        Body = body;
        DropNotificationStatusId = dropNotificationStatusId;
        ScheduledAt = scheduledAt;
        SentAt = sentAt;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    #endregion
}
