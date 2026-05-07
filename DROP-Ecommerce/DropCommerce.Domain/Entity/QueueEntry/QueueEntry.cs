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
        BaseValidate<long>.ValidateNotNullValue(dropEventId);
        BaseValidate<long>.ValidateIdValue(dropEventId);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(sessionToken);

        BaseValidate<int>.ValidateNotNullValue(position);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(deviceFingerprint);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(ipAddress);
        BaseValidate<string>.ValidateStringWhiteSpaceValue(userAgent);

        BaseValidate<DateTime>.ValidateNotNullValue(enteredAt);

        return new QueueEntry(dropEventId, customerId, sessionToken, position, statusId, deviceFingerprint, ipAddress, userAgent, enteredAt, calledAt, expiredAt, checkedOutAt);
    }

    #endregion
}