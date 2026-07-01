using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class WaitlistEntry : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long? DropProductId { get; private set; }
    public long CustomerId { get; private set; }
    public int Position { get; private set; }
    public long WaitlistEntryStatusId { get; private set; }
    public bool NotificationSent { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public DropProduct? DropProduct { get; private set; }
    public WaitlistEntryStatus WaitlistEntryStatus { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected WaitlistEntry() { }

    private WaitlistEntry(long dropEventId, long? dropProductId, long customerId, int position, long waitlistEntryStatusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt)
    {
        DropEventId = dropEventId;
        DropProductId = dropProductId;
        CustomerId = customerId;
        Position = position;
        WaitlistEntryStatusId = waitlistEntryStatusId;
        NotificationSent = notificationSent;
        JoinedAt = joinedAt;
        NotifiedAt = notifiedAt;
        ExpiresAt = expiresAt;
    }

    #endregion

    #region Functions

    public static WaitlistEntry Create(long dropEventId, long? dropProductId, long customerId, int position, long waitlistEntryStatusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateIdNullable(dropProductId, nameof(dropProductId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateMinimum(position, 1, nameof(position));
        BaseValidate.ValidateId(waitlistEntryStatusId, nameof(waitlistEntryStatusId));
        BaseValidate.ValidateDate(joinedAt, nameof(joinedAt));
        BaseValidate.ValidateDate(expiresAt, nameof(expiresAt));
        BaseValidate.ValidateDateRange(joinedAt, expiresAt, nameof(joinedAt), nameof(expiresAt));

        return new WaitlistEntry(dropEventId, dropProductId, customerId, position, waitlistEntryStatusId, notificationSent, joinedAt, notifiedAt, expiresAt);
    }

    public void Update(long dropEventId, long? dropProductId, long customerId, int position, long waitlistEntryStatusId, bool notificationSent, DateTime joinedAt, DateTime? notifiedAt, DateTime expiresAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateIdNullable(dropProductId, nameof(dropProductId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateMinimum(position, 1, nameof(position));
        BaseValidate.ValidateId(waitlistEntryStatusId, nameof(waitlistEntryStatusId));
        BaseValidate.ValidateDate(joinedAt, nameof(joinedAt));
        BaseValidate.ValidateDate(expiresAt, nameof(expiresAt));
        BaseValidate.ValidateDateRange(joinedAt, expiresAt, nameof(joinedAt), nameof(expiresAt));

        DropEventId = dropEventId;
        DropProductId = dropProductId;
        CustomerId = customerId;
        Position = position;
        WaitlistEntryStatusId = waitlistEntryStatusId;
        NotificationSent = notificationSent;
        JoinedAt = joinedAt;
        NotifiedAt = notifiedAt;
        ExpiresAt = expiresAt;
    }

    #endregion
}
