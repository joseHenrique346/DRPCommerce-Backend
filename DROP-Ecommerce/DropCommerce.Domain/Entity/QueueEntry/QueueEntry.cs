namespace DropCommerce.Domain.Entity;

public class QueueEntry : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public string SessionToken { get; private set; }
    public int Position { get; private set; }
    public long StatusId { get; private set; }
    public string DeviceFingerprint { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime EnteredAt { get; private set; }
    public DateTime? CalledAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public DateTime? CheckedOutAt { get; private set; }

    #endregion

    #region Constructors

    protected QueueEntry() { }

    private QueueEntry(long dropEventId, long customerId, string sessionToken, int position, long statusId, string deviceFingerprint, string ipAddress, string userAgent, DateTime enteredAt, DateTime? calledAt, DateTime? expiredAt, DateTime? checkedOutAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        SessionToken = sessionToken;
        Position = position;
        StatusId = statusId;
        DeviceFingerprint = deviceFingerprint;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        EnteredAt = enteredAt;
        CalledAt = calledAt;
        ExpiredAt = expiredAt;
        CheckedOutAt = checkedOutAt;
    }

    #endregion

    #region Functions

    public static QueueEntry Create(long dropEventId, long customerId, string sessionToken, int position, long statusId, string deviceFingerprint, string ipAddress, string userAgent, DateTime enteredAt, DateTime? calledAt, DateTime? expiredAt, DateTime? checkedOutAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateString(sessionToken, nameof(sessionToken));
        BaseValidate.ValidateMinimum(position, 1, nameof(position));
        BaseValidate.ValidateId(statusId, nameof(statusId));
        BaseValidate.ValidateString(deviceFingerprint, nameof(deviceFingerprint));
        BaseValidate.ValidateRegexString(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$", nameof(ipAddress));
        BaseValidate.ValidateString(userAgent, nameof(userAgent));
        BaseValidate.ValidateDate(enteredAt, nameof(enteredAt));

        return new QueueEntry(dropEventId, customerId, sessionToken, position, statusId, deviceFingerprint, ipAddress, userAgent, enteredAt, calledAt, expiredAt, checkedOutAt);
    }

    #endregion
}
