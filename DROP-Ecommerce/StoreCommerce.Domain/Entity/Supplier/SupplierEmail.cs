namespace StoreCommerce.Domain.Entity;

public class SupplierEmail
{
    public string Value { get; private set; }

    public SupplierEmail() { }

    public SupplierEmail(string value)
    {
        Value = value;
    }
}