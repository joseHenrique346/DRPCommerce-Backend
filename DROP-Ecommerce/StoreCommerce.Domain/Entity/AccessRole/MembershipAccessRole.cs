using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class MembershipAccessRole : ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long EnterpriseMembershipId { get; private set; }
    public long AccessRoleId { get; private set; }
    public EnterpriseMembership EnterpriseMembership { get; private set; } = null!;
    public AccessRole AccessRole { get; private set; } = null!;

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

    public static MembershipAccessRole Create(long enterpriseId, long enterpriseMembershipId, long accessRoleId) =>
        new(enterpriseId, enterpriseMembershipId, accessRoleId);
}
