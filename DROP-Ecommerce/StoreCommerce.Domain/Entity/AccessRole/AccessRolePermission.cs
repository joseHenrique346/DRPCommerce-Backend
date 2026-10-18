using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class AccessRolePermission : ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long AccessRoleId { get; private set; }
    public long PermissionId { get; private set; }
    public AccessRole AccessRole { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    protected AccessRolePermission() { }

    private AccessRolePermission(long enterpriseId, long accessRoleId, long permissionId)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(accessRoleId, nameof(AccessRoleId));
        BaseValidate.ValidatePositive(permissionId, nameof(PermissionId));
        EnterpriseId = enterpriseId;
        AccessRoleId = accessRoleId;
        PermissionId = permissionId;
    }

    public static AccessRolePermission Create(long enterpriseId, long accessRoleId, long permissionId) =>
        new(enterpriseId, accessRoleId, permissionId);
}
