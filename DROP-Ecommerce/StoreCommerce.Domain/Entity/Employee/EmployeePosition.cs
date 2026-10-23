using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class EmployeePosition : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; } = null!;
    private readonly List<Employee> _listEmployee = [];
    public IReadOnlyCollection<Employee> ListEmployee => _listEmployee.AsReadOnly();
    #endregion

    #endregion

    #region Constructors
    protected EmployeePosition() { }

    private EmployeePosition(long enterpriseId, string name, string description, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));

        EnterpriseId = enterpriseId;
        UpdateDetails(name, description, isActive);
    }
    #endregion

    #region Functions
    public static EmployeePosition Create(long enterpriseId, string name, string description, bool isActive)
    {
        return new EmployeePosition(enterpriseId, name, description, isActive);
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