namespace StoreCommerce.Domain.Entity;

public class EmployeeEmail
{
    public string Value { get; private set; }

    public EmployeeEmail() { }

    public EmployeeEmail(string value)
    {
        Value = value;
    }
}