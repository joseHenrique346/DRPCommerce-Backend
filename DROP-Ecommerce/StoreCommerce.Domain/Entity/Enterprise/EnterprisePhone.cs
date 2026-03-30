namespace StoreCommerce.Domain.Entity;

public class EnterprisePhone
{
    public string Value { get; private set; }

    public EnterprisePhone() { }

    public EnterprisePhone(string value)
    {
        Value = value;
    }
}