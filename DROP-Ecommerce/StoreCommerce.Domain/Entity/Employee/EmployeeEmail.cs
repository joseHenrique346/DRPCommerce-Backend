using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class EmployeeEmail
{
    #region Properties
    public string Value { get; private set; }
    #endregion

    #region Constructor
    protected EmployeeEmail() { }

    private EmployeeEmail(string value)
    {
        Value = value;
    }
    #endregion

    #region Functions
    public static EmployeeEmail Create(string value)
    {
        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Value));
        BaseValidate.ValidateMaxLength(value, 255, nameof(Value));
        BaseValidate.ValidateEmailFormat(value, nameof(Value));

        return new EmployeeEmail(value);
    }
    #endregion
}