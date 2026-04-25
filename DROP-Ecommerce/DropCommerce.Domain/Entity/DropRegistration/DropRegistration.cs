namespace DropCommerce.Domain.Entity;

public class DropRegistration : BaseEntity
{
    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long StatusId { get; private set; }
    public bool IsEligible { get; private set; }
    public string EligibilityReason { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? EligibilityCheckedAt { get; private set; }

    public DropRegistration() { }

    public DropRegistration(long dropEventId, long customerId, long statusId, bool isEligible, string eligibilityReason, DateTime registeredAt, DateTime? eligibilityCheckedAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        StatusId = statusId;
        IsEligible = isEligible;
        EligibilityReason = eligibilityReason;
        RegisteredAt = registeredAt;
        EligibilityCheckedAt = eligibilityCheckedAt;
    }
}
