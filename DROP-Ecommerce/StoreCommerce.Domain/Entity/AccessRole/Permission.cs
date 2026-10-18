using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Permission : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private readonly List<AccessRolePermission> _listAccessRolePermission = [];
    public IReadOnlyCollection<AccessRolePermission> ListAccessRolePermission => _listAccessRolePermission.AsReadOnly();

    protected Permission() { }

    private Permission(string code, string name, string description, string module, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(code, nameof(Code));
        BaseValidate.ValidateMaxLength(code, 150, nameof(Code));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateMaxLength(description, 1000, nameof(Description));
        BaseValidate.ValidateNotNullOrEmpty(module, nameof(Module));
        BaseValidate.ValidateMaxLength(module, 100, nameof(Module));
        Code = code;
        Name = name;
        Description = description;
        Module = module;
        IsActive = isActive;
    }

    public static Permission Create(string code, string name, string description, string module, bool isActive) =>
        new(code, name, description, module, isActive);
}
