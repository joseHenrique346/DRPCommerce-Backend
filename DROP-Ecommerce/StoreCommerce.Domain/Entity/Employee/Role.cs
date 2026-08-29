using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Role : BaseEntity
{
    #region Properties
    public string Name { get; private set; }
    public string Description { get; private set; }

    #region Navigation Properties
    private readonly List<Employee> _listEmployee = [];
    public IReadOnlyCollection<Employee> ListEmployee => _listEmployee.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Role() { }

    private Role(string name, string description)
    {
        Name = name;
        Description = description;
    }
    #endregion

    #region Functions
    public static Role Create(string name, string description)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateMaxLength(description, 1000, nameof(Description));

        return new Role(name, description);
    }

    public void UpdateDetails(string name, string description)
    {
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 255, nameof(Name));
        BaseValidate.ValidateMaxLength(description, 1000, nameof(Description));

        Name = name;
        Description = description;
    }
    #endregion
}
