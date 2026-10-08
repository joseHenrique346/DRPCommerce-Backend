using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public sealed class UserEmail
{
    public string Value { get; private set; } = string.Empty;
    private UserEmail() { }
    private UserEmail(string value) => Value = value;

    public static UserEmail Create(string value)
    {
        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Value));
        BaseValidate.ValidateMaxLength(value, 255, nameof(Value));
        BaseValidate.ValidateEmailFormat(value, nameof(Value));
        return new UserEmail(value.Trim());
    }
}
