using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class Enterprise : BaseEntity
{
    #region Properties
    public string TradeName { get; private set; }
    public string LegalName { get; private set; }
    public EnterpriseEmail Email { get; private set; }
    public EnterprisePhone Phone { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public long StateId { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public bool IsActive { get; private set; }
    #endregion

    #region Constructor
    protected Enterprise() { }

    private Enterprise(string tradeName, string legalName, EnterpriseEmail email, EnterprisePhone phone, string addressLine, string city, long stateId, string zipCode, string country, bool isActive)
    {
        TradeName = tradeName;
        LegalName = legalName;
        Email = email;
        Phone = phone;
        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    public static Enterprise Create(string tradeName, string legalName, EnterpriseEmail email, EnterprisePhone phone, string addressLine, string city, long stateId, string zipCode, string country, bool isActive)
    {
        BaseValidate.ValidateNotNullOrEmpty(tradeName, nameof(TradeName));
        BaseValidate.ValidateMaxLength(tradeName, 255, nameof(TradeName));
        BaseValidate.ValidateNotNullOrEmpty(legalName, nameof(LegalName));
        BaseValidate.ValidateMaxLength(legalName, 255, nameof(LegalName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        return new Enterprise(tradeName, legalName, email, phone, addressLine, city, stateId, zipCode, country, isActive);
    }

    public void UpdateInfo(string tradeName, string legalName, EnterpriseEmail email, EnterprisePhone phone)
    {
        BaseValidate.ValidateNotNullOrEmpty(tradeName, nameof(TradeName));
        BaseValidate.ValidateMaxLength(tradeName, 255, nameof(TradeName));
        BaseValidate.ValidateNotNullOrEmpty(legalName, nameof(LegalName));
        BaseValidate.ValidateMaxLength(legalName, 255, nameof(LegalName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));

        TradeName = tradeName;
        LegalName = legalName;
        Email = email;
        Phone = phone;
    }

    public void UpdateAddress(string addressLine, string city, long stateId, string zipCode, string country)
    {
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
    }
    #endregion
}
