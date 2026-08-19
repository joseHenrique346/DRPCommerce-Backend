using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class EnterpriseEmail
{
    #region Properties
    public string Value { get; private set; }
    #endregion

    #region Constructor
    protected EnterpriseEmail() { }

    private EnterpriseEmail(string value)
    {
        Value = value;
    }
    #endregion

    #region Functions
    public static EnterpriseEmail Create(string value)
    {
        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Value));
        BaseValidate.ValidateMaxLength(value, 255, nameof(Value));
        BaseValidate.ValidateEmailFormat(value, nameof(Value));

        return new EnterpriseEmail(value);
    }
    #endregion
}