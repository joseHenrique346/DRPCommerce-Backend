using System.Text.RegularExpressions;

namespace StoreCommerce.Domain.Entity;

public class CustomerEmail
{
    #region Properties
    public string Value { get; private set; }
    #endregion

    #region Constructor
    protected CustomerEmail() { }

    private CustomerEmail(string value)
    {
        Value = value;
    }
    #endregion

    #region Functions
    public static CustomerEmail Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{nameof(Value)} não pode ser vazia.", nameof(Value));
        if (value.Length > 255)
            throw new ArgumentException($"{nameof(Value)} não pode ter mais de 255 caracteres.", nameof(Value));
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            throw new ArgumentException($"{nameof(Value)} possui formato de e-mail inválido.", nameof(Value));

        return new CustomerEmail(value);
    }
    #endregion
}
