using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class AccessRolePermission : ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long AccessRoleId { get; private set; }
    public long PermissionId { get; private set; }

    #region Navigation Properties
    public AccessRole AccessRole { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
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
    #endregion

    #region Functions
    public static AccessRolePermission Create(long enterpriseId, long accessRoleId, long permissionId)
    {
        return new AccessRolePermission(enterpriseId, accessRoleId, permissionId);
    }
    #endregion
}