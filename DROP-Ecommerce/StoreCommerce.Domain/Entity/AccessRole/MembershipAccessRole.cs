using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class MembershipAccessRole : ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long EnterpriseMembershipId { get; private set; }
    public long AccessRoleId { get; private set; }

    #region Navigation Properties
    public EnterpriseMembership EnterpriseMembership { get; private set; } = null!;
    public AccessRole AccessRole { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    protected MembershipAccessRole() { }

    private MembershipAccessRole(long enterpriseId, long enterpriseMembershipId, long accessRoleId)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(enterpriseMembershipId, nameof(EnterpriseMembershipId));
        BaseValidate.ValidatePositive(accessRoleId, nameof(AccessRoleId));

        EnterpriseId = enterpriseId;
        EnterpriseMembershipId = enterpriseMembershipId;
        AccessRoleId = accessRoleId;
    }
    #endregion

    #region Functions
    public static MembershipAccessRole Create(long enterpriseId, long enterpriseMembershipId, long accessRoleId)
    {
        return new MembershipAccessRole(enterpriseId, enterpriseMembershipId, accessRoleId);
    }
    #endregion
}