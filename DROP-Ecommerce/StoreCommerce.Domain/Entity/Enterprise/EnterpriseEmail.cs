namespace StoreCommerce.Domain.Entity;

public class EnterpriseEmail
{
    public string Value { get; private set; }

    public EnterpriseEmail() { }

    public EnterpriseEmail(string value)
    {
        Value = value;
    }
}