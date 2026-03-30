namespace StoreCommerce.Domain.Entity;

public class Supplier : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public string CompanyName { get; private set; }
    public string ContactName { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public bool IsActive { get; private set; }

    public Supplier() { }

    public Supplier(long enterpriseId, string companyName, string contactName, string addressLine, string city, string state, string zipCode, string country, bool isActive)
    {
        EnterpriseId = enterpriseId;
        CompanyName = companyName;
        ContactName = contactName;
        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        IsActive = isActive;
    }
}