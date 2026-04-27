namespace DropCommerce.Domain.Entity;

public class FraudSignal : BaseEntity
{
    public long CustomerId { get; private set; }
    public long DropEventId { get; private set; }
    public long QueueEntryId { get; private set; }
    public long SignalTypeId { get; private set; }
    public long SeverityId { get; private set; }
    public string Description { get; private set; }
    public string IpAddress { get; private set; }
    public string DeviceFingerprint { get; private set; }
    public bool IsConfirmed { get; private set; }
    public bool WasBlocked { get; private set; }
    public DateTime DetectedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    public FraudSignal() { }

    public FraudSignal(long customerId, long dropEventId, long queueEntryId, long signalTypeId, long severityId, string description, string ipAddress, string deviceFingerprint, bool isConfirmed, bool wasBlocked, DateTime detectedAt, DateTime? reviewedAt)
    {
        CustomerId = customerId;
        DropEventId = dropEventId;
        QueueEntryId = queueEntryId;
        SignalTypeId = signalTypeId;
        SeverityId = severityId;
        Description = description;
        IpAddress = ipAddress;
        DeviceFingerprint = deviceFingerprint;
        IsConfirmed = isConfirmed;
        WasBlocked = wasBlocked;
        DetectedAt = detectedAt;
        ReviewedAt = reviewedAt;
    }
}
