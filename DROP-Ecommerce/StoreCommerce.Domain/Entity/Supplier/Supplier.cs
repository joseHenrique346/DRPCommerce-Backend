using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Supplier : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string CompanyName { get; private set; }
    public string ContactName { get; private set; }
    public SupplierEmail Email { get; private set; }
    public SupplierPhone Phone { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public long StateId { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public bool IsActive { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public StaticEntity.State State { get; private set; }
    private readonly List<Product> _listProduct = [];
    public IReadOnlyCollection<Product> ListProduct => _listProduct.AsReadOnly();
    private readonly List<Shipment> _listShipment = [];
    public IReadOnlyCollection<Shipment> ListShipment => _listShipment.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Supplier() { }

    private Supplier(long enterpriseId, string companyName, string contactName, SupplierEmail email, SupplierPhone phone, string addressLine, string city, long stateId, string zipCode, string country, bool isActive)
    {
        EnterpriseId = enterpriseId;
        CompanyName = companyName;
        ContactName = contactName;
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
    public static Supplier Create(long enterpriseId, string companyName, string contactName, SupplierEmail email, SupplierPhone phone, string addressLine, string city, long stateId, string zipCode, string country, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(companyName, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(companyName, 255, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(contactName, 255, nameof(ContactName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));

        return new Supplier(enterpriseId, companyName, contactName, email, phone, addressLine, city, stateId, zipCode, country, isActive);
    }

    public void UpdateInfo(string companyName, string contactName, SupplierEmail email, SupplierPhone phone)
    {
        BaseValidate.ValidateNotNullOrEmpty(companyName, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(companyName, 255, nameof(CompanyName));
        BaseValidate.ValidateMaxLength(contactName, 255, nameof(ContactName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));

        CompanyName = companyName;
        ContactName = contactName;
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
