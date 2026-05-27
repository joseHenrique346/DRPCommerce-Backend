using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class DropRegistration : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long CustomerId { get; private set; }
    public long DropRegistrationStatusId { get; private set; }
    public bool IsEligible { get; private set; }
    public string EligibilityReason { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? EligibilityCheckedAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }
    public DropRegistrationStatus DropRegistrationStatus { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected DropRegistration() { }

    private DropRegistration(long dropEventId, long customerId, long dropRegistrationStatusId, bool isEligible, string eligibilityReason, DateTime registeredAt, DateTime? eligibilityCheckedAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        DropRegistrationStatusId = dropRegistrationStatusId;
        IsEligible = isEligible;
        EligibilityReason = eligibilityReason;
        RegisteredAt = registeredAt;
        EligibilityCheckedAt = eligibilityCheckedAt;
    }

    #endregion

    #region Functions

    public static DropRegistration Create(long dropEventId, long customerId, long dropRegistrationStatusId, bool isEligible, string eligibilityReason, DateTime registeredAt, DateTime? eligibilityCheckedAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateId(dropRegistrationStatusId, nameof(dropRegistrationStatusId));
        BaseValidate.ValidateString(eligibilityReason, nameof(eligibilityReason));
        BaseValidate.ValidateDate(registeredAt, nameof(registeredAt));

        return new DropRegistration(dropEventId, customerId, dropRegistrationStatusId, isEligible, eligibilityReason, registeredAt, eligibilityCheckedAt);
    }

    #endregion
}
