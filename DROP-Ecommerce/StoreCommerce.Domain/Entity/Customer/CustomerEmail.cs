namespace StoreCommerce.Domain.Entity;

public class CustomerEmail
{
    public string Value { get; private set; }

    public CustomerEmail() { }

    public CustomerEmail(string value)
    {
        Value = value;
    }
}