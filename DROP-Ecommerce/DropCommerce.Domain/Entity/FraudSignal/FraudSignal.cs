using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class FraudSignal : BaseEntity
{
    #region Properties

    public long CustomerId { get; private set; }
    public long DropEventId { get; private set; }
    public long QueueEntryId { get; private set; }
    public long FraudSignalTypeId { get; private set; }
    public long FraudSeverityId { get; private set; }
    public string Description { get; private set; }
    public string IpAddress { get; private set; }
    public string DeviceFingerprint { get; private set; }
    public bool IsConfirmed { get; private set; }
    public bool WasBlocked { get; private set; }
    public DateTime DetectedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public QueueEntry QueueEntry { get; private set; }
    public FraudSignalType FraudSignalType { get; private set; }
    public FraudSeverity FraudSeverity { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected FraudSignal() { }

    private FraudSignal(long customerId, long dropEventId, long queueEntryId, long fraudSignalTypeId, long fraudSeverityId, string description, string ipAddress, string deviceFingerprint, bool isConfirmed, bool wasBlocked, DateTime detectedAt, DateTime? reviewedAt)
    {
        CustomerId = customerId;
        DropEventId = dropEventId;
        QueueEntryId = queueEntryId;
        FraudSignalTypeId = fraudSignalTypeId;
        FraudSeverityId = fraudSeverityId;
        Description = description;
        IpAddress = ipAddress;
        DeviceFingerprint = deviceFingerprint;
        IsConfirmed = isConfirmed;
        WasBlocked = wasBlocked;
        DetectedAt = detectedAt;
        ReviewedAt = reviewedAt;
    }

    #endregion

    #region Functions

    public static FraudSignal Create(long customerId, long dropEventId, long queueEntryId, long fraudSignalTypeId, long fraudSeverityId, string description, string ipAddress, string deviceFingerprint, bool isConfirmed, bool wasBlocked, DateTime detectedAt, DateTime? reviewedAt)
    {
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(queueEntryId, nameof(queueEntryId));
        BaseValidate.ValidateId(fraudSignalTypeId, nameof(fraudSignalTypeId));
        BaseValidate.ValidateId(fraudSeverityId, nameof(fraudSeverityId));
        BaseValidate.ValidateString(description, nameof(description));
        BaseValidate.ValidateRegexString(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$", nameof(ipAddress));
        BaseValidate.ValidateString(deviceFingerprint, nameof(deviceFingerprint));
        BaseValidate.ValidateDate(detectedAt, nameof(detectedAt));

        return new FraudSignal(customerId, dropEventId, queueEntryId, fraudSignalTypeId, fraudSeverityId, description, ipAddress, deviceFingerprint, isConfirmed, wasBlocked, detectedAt, reviewedAt);
    }

    #endregion
}
