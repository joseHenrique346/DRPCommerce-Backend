using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Entity;

public class EnterpriseMembership : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long UserId { get; private set; }
    public long KindId { get; private set; }
    public bool IsActive { get; private set; }
    public long AccessVersion { get; private set; } = 1;
    public DateTime JoinedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public EnterpriseMembershipKind Kind { get; private set; } = null!;
    public Employee? Employee { get; private set; }
    private readonly List<MembershipAccessRole> _listMembershipAccessRole = [];
    public IReadOnlyCollection<MembershipAccessRole> ListMembershipAccessRole => _listMembershipAccessRole.AsReadOnly();
    #endregion

    #endregion

    #region Constructors
    protected EnterpriseMembership() { }

    private EnterpriseMembership(long enterpriseId, long userId, long kindId, bool isActive, DateTime joinedAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(userId, nameof(UserId));

        EnterpriseId = enterpriseId;
        UserId = userId;
        UpdateDetails(kindId, isActive, joinedAt);
        AccessVersion = 1;
    }
    #endregion

    #region Functions
    public static EnterpriseMembership Create(long enterpriseId, long userId, long kindId, bool isActive, DateTime joinedAt)
    {
        return new EnterpriseMembership(enterpriseId, userId, kindId, isActive, joinedAt);
    }

    public void UpdateDetails(long kindId, bool isActive, DateTime joinedAt)
    {
        BaseValidate.ValidateRange(kindId, 1, 3, nameof(KindId));

        if (KindId != kindId || IsActive != isActive)
            InvalidateAccess();

        KindId = kindId;
        IsActive = isActive;
        JoinedAt = joinedAt;
    }

    public void InvalidateAccess() => AccessVersion = checked(AccessVersion + 1);
    #endregion
}