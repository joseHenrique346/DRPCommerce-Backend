using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class QueueEntry : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public string SessionToken { get; private set; }
    public int Position { get; private set; }
    public long QueueEntryStatusId { get; private set; }
    public string DeviceFingerprint { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime EnteredAt { get; private set; }
    public DateTime? CalledAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public DateTime? CheckedOutAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public QueueEntryStatus QueueEntryStatus { get; private set; }
    public ICollection<QueueSession> ListQueueSession { get; private set; } = [];
    public ICollection<DropReservation> ListDropReservation { get; private set; } = [];
    public ICollection<FraudSignal> ListFraudSignal { get; private set; } = [];

    #endregion

    #endregion

    #region Constructors

    protected QueueEntry() { }

    private QueueEntry(long dropEventId, long customerId, string sessionToken, int position, long queueEntryStatusId, string deviceFingerprint, string ipAddress, string userAgent, DateTime enteredAt, DateTime? calledAt, DateTime? expiredAt, DateTime? checkedOutAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        SessionToken = sessionToken;
        Position = position;
        QueueEntryStatusId = queueEntryStatusId;
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

    public static QueueEntry Create(long dropEventId, long customerId, string sessionToken, int position, long queueEntryStatusId, string deviceFingerprint, string ipAddress, string userAgent, DateTime enteredAt, DateTime? calledAt, DateTime? expiredAt, DateTime? checkedOutAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateString(sessionToken, nameof(sessionToken));
        BaseValidate.ValidateMinimum(position, 1, nameof(position));
        BaseValidate.ValidateId(queueEntryStatusId, nameof(queueEntryStatusId));
        BaseValidate.ValidateString(deviceFingerprint, nameof(deviceFingerprint));
        BaseValidate.ValidateRegexString(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$", nameof(ipAddress));
        BaseValidate.ValidateString(userAgent, nameof(userAgent));
        BaseValidate.ValidateDate(enteredAt, nameof(enteredAt));

        return new QueueEntry(dropEventId, customerId, sessionToken, position, queueEntryStatusId, deviceFingerprint, ipAddress, userAgent, enteredAt, calledAt, expiredAt, checkedOutAt);
    }

    public void Update(long dropEventId, long customerId, string sessionToken, int position, long queueEntryStatusId, string deviceFingerprint, string ipAddress, string userAgent, DateTime enteredAt, DateTime? calledAt, DateTime? expiredAt, DateTime? checkedOutAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateString(sessionToken, nameof(sessionToken));
        BaseValidate.ValidateMinimum(position, 1, nameof(position));
        BaseValidate.ValidateId(queueEntryStatusId, nameof(queueEntryStatusId));
        BaseValidate.ValidateString(deviceFingerprint, nameof(deviceFingerprint));
        BaseValidate.ValidateRegexString(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$", nameof(ipAddress));
        BaseValidate.ValidateString(userAgent, nameof(userAgent));
        BaseValidate.ValidateDate(enteredAt, nameof(enteredAt));

        DropEventId = dropEventId;
        CustomerId = customerId;
        SessionToken = sessionToken;
        Position = position;
        QueueEntryStatusId = queueEntryStatusId;
        DeviceFingerprint = deviceFingerprint;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        EnteredAt = enteredAt;
        CalledAt = calledAt;
        ExpiredAt = expiredAt;
        CheckedOutAt = checkedOutAt;
    }

    #endregion
}
