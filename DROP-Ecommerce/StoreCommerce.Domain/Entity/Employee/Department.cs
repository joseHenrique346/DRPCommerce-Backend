using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Department : BaseEntity, ITenantEntity
{
    #region Constants
    public const int NameMaxLength = 255;
    public const int NormalizedNameMaxLength = 255;
    public const int DescriptionMaxLength = 1000;
    #endregion

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
    protected Department() { }

    private Department(long enterpriseId, string name, string description, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));

        EnterpriseId = enterpriseId;
        UpdateDetails(name, description, isActive);
    }
    #endregion

    #region Functions
    public static Department Create(long enterpriseId, string name, string description, bool isActive)
    {
        return new Department(enterpriseId, name, description, isActive);
    }

    public void UpdateDetails(string name, string description, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, NameMaxLength, nameof(Name));
        BaseValidate.ValidateMaxLength(description, DescriptionMaxLength, nameof(Description));

        Name = name;
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description;
        IsActive = isActive;
    }
    #endregion
}