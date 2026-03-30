namespace StoreCommerce.Domain.Entity;

public class CustomerPhone
{
    public string Value { get; private set; }

    public CustomerPhone() { }

    public CustomerPhone(string value)
    {
        Value = value;
    }
}