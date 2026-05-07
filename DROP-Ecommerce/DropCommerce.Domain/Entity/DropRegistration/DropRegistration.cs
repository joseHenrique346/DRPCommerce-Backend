namespace DropCommerce.Domain.Entity;

public class DropRegistration : BaseEntity
{
    #region Properties
    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long StatusId { get; private set; }
    public bool IsEligible { get; private set; }
    public string EligibilityReason { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? EligibilityCheckedAt { get; private set; }

    #endregion

    #region Constructors

    protected DropRegistration() { }

    private DropRegistration(long dropEventId, long customerId, long statusId, bool isEligible, string eligibilityReason, DateTime registeredAt, DateTime? eligibilityCheckedAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        StatusId = statusId;
        IsEligible = isEligible;
        EligibilityReason = eligibilityReason;
        RegisteredAt = registeredAt;
        EligibilityCheckedAt = eligibilityCheckedAt;
    }

    #endregion

    #region Properties

    public static DropRegistration Create(long dropEventId, long customerId, long statusId, bool isEligible, string eligibilityReason, DateTime registeredAt, DateTime? eligibilityCheckedAt)
    {
        BaseValidate<long>.ValidateNotNullValue(dropEventId);
        BaseValidate<long>.ValidateIdValue(dropEventId);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<long>.ValidateNotNullValue(statusId);
        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<bool>.ValidateNotNullValue(isEligible);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(eligibilityReason);

        BaseValidate<DateTime>.ValidateNotNullValue(registeredAt);

        return new DropRegistration(dropEventId, customerId, statusId, isEligible, eligibilityReason, registeredAt, eligibilityCheckedAt);
    }

    #endregion
}