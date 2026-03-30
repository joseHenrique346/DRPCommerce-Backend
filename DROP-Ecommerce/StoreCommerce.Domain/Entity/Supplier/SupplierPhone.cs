namespace StoreCommerce.Domain.Entity;

public class SupplierPhone
{
    public string Value { get; private set; }

    public SupplierPhone() { }

    public SupplierPhone(string value)
    {
        Value = value;
    }
}