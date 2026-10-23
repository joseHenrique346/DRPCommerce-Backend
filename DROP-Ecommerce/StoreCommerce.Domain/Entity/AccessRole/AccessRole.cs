using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class AccessRole : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; } = null!;
    private readonly List<AccessRolePermission> _listAccessRolePermission = [];
    public IReadOnlyCollection<AccessRolePermission> ListAccessRolePermission => _listAccessRolePermission.AsReadOnly();
    private readonly List<MembershipAccessRole> _listMembershipAccessRole = [];
    public IReadOnlyCollection<MembershipAccessRole> ListMembershipAccessRole => _listMembershipAccessRole.AsReadOnly();
    #endregion

    #endregion

    #region Constructors
    protected AccessRole() { }

    private AccessRole(long enterpriseId, string name, string description, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));

        EnterpriseId = enterpriseId;
        UpdateDetails(name, description, isActive);
    }
    #endregion

    #region Functions
    public static AccessRole Create(long enterpriseId, string name, string description, bool isActive)
    {
        return new AccessRole(enterpriseId, name, description, isActive);
    }

    public void UpdateDetails(string name, string description, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateMaxLength(description, 1000, nameof(Description));

        Name = name;
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description;
        IsActive = isActive;
    }
    #endregion
}