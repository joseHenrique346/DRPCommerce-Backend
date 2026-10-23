using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public sealed class UserEmail
{
    #region Properties
    public string Value { get; private set; } = string.Empty;
    #endregion

    #region Constructors
    private UserEmail() { }

    private UserEmail(string value)
    {
        Value = value;
    }
    #endregion

    #region Functions
    public static UserEmail Create(string value)
    {
        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Value));
        BaseValidate.ValidateMaxLength(value, 255, nameof(Value));
        BaseValidate.ValidateEmailFormat(value, nameof(Value));

        return new UserEmail(value.Trim());
    }
    #endregion
}