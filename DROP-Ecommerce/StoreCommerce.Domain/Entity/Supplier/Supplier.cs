using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Supplier : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string CompanyName { get; private set; }
    public string ContactName { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public long StateId { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public bool IsActive { get; private set; }
    #endregion

    #region Constructor
    protected Supplier() { }

    private Supplier(long enterpriseId, string companyName, string contactName, string addressLine, string city, string state, string zipCode, string country, bool isActive)
    {
        EnterpriseId = enterpriseId;
        CompanyName = companyName;
        ContactName = contactName;
        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    public static Supplier Create(long enterpriseId, string companyName, string contactName, string addressLine, string city, string state, string zipCode, string country, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(companyName, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(companyName, 255, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(contactName, 255, nameof(ContactName));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidateMaxLength(state, 255, nameof(State));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        return new Supplier(enterpriseId, companyName, contactName, addressLine, city, state, zipCode, country, isActive);
    }

    public void UpdateInfo(string companyName, string contactName)
    {
        BaseValidate.ValidateNotNullOrEmpty(companyName, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(companyName, 255, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(contactName, 255, nameof(ContactName));

        CompanyName = companyName;
        ContactName = contactName;
    }

    public void UpdateAddress(string addressLine, string city, string state, string zipCode, string country)
    {
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidateMaxLength(state, 255, nameof(State));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }
    #endregion
}
