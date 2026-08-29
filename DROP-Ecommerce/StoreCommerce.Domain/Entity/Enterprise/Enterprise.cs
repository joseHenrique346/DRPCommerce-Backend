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

    #region Navigation Properties
    public StaticEntity.State State { get; private set; }
    private readonly List<Category> _listCategory = [];
    public IReadOnlyCollection<Category> ListCategory => _listCategory.AsReadOnly();
    private readonly List<Coupon> _listCoupon = [];
    public IReadOnlyCollection<Coupon> ListCoupon => _listCoupon.AsReadOnly();
    private readonly List<Customer> _listCustomer = [];
    public IReadOnlyCollection<Customer> ListCustomer => _listCustomer.AsReadOnly();
    private readonly List<Document> _listDocument = [];
    public IReadOnlyCollection<Document> ListDocument => _listDocument.AsReadOnly();
    private readonly List<Employee> _listEmployee = [];
    public IReadOnlyCollection<Employee> ListEmployee => _listEmployee.AsReadOnly();
    private readonly List<Invoice> _listInvoice = [];
    public IReadOnlyCollection<Invoice> ListInvoice => _listInvoice.AsReadOnly();
    private readonly List<Order> _listOrder = [];
    public IReadOnlyCollection<Order> ListOrder => _listOrder.AsReadOnly();
    private readonly List<Product> _listProduct = [];
    public IReadOnlyCollection<Product> ListProduct => _listProduct.AsReadOnly();
    private readonly List<Service> _listService = [];
    public IReadOnlyCollection<Service> ListService => _listService.AsReadOnly();
    private readonly List<Supplier> _listSupplier = [];
    public IReadOnlyCollection<Supplier> ListSupplier => _listSupplier.AsReadOnly();
    #endregion

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
