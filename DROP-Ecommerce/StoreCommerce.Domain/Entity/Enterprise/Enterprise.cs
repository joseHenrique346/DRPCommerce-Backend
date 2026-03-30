namespace StoreCommerce.Domain.Entity;

public class Enterprise : BaseEntity
{
    public string TradeName { get; private set; }
    public string LegalName { get; private set; }
    public EnterpriseEmail Email { get; private set; }
    public EnterprisePhone Phone { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public bool IsActive { get; private set; }

    public Enterprise() { }

    public Enterprise(string tradeName, string legalName, EnterpriseEmail email, EnterprisePhone phone, string addressLine, string city, string state, string zipCode, string country, bool isActive)
    {
        TradeName = tradeName;
        LegalName = legalName;
        Email = email;
        Phone = phone;
        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        IsActive = isActive;
    }
}