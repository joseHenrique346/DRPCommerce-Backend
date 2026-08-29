using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Customer : BaseEntity, ISoftDeletable, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string FullName { get; private set; }
    public CustomerEmail Email { get; private set; }
    public CustomerPhone Phone { get; private set; }
    public string PasswordHash { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public long StateId { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public string Gender { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public StaticEntity.State State { get; private set; }
    private readonly List<Order> _listOrder = [];
    public IReadOnlyCollection<Order> ListOrder => _listOrder.AsReadOnly();
    private readonly List<Invoice> _listInvoice = [];
    public IReadOnlyCollection<Invoice> ListInvoice => _listInvoice.AsReadOnly();
    private readonly List<Transaction> _listTransaction = [];
    public IReadOnlyCollection<Transaction> ListTransaction => _listTransaction.AsReadOnly();
    private readonly List<Document> _listDocument = [];
    public IReadOnlyCollection<Document> ListDocument => _listDocument.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Customer() { }

    private Customer(long enterpriseId, string fullName, CustomerEmail email, CustomerPhone phone, string passwordHash, string addressLine, string city, long stateId, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        EnterpriseId = enterpriseId;
        FullName = fullName;
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        IsVerified = isVerified;
        IsActive = isActive;
    }
    #endregion

    #region Functions
    public static Customer Create(long enterpriseId, string fullName, CustomerEmail email, CustomerPhone phone, string passwordHash, string addressLine, string city, long stateId, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, 500, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));
        BaseValidate.ValidateMaxLength(gender, 50, nameof(Gender));
        BaseValidate.ValidateNotFuture(dateOfBirth, nameof(DateOfBirth));

        return new Customer(enterpriseId, fullName, email, phone, passwordHash, addressLine, city, stateId, zipCode, country, gender, dateOfBirth, isVerified, isActive);
    }

    public void UpdatePersonalInfo(string fullName, CustomerEmail email, CustomerPhone phone, string gender, DateTime dateOfBirth)
    {
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));
        BaseValidate.ValidateMaxLength(gender, 50, nameof(Gender));
        BaseValidate.ValidateNotFuture(dateOfBirth, nameof(DateOfBirth));

        FullName = fullName;
        Email = email;
        Phone = phone;
        Gender = gender;
        DateOfBirth = dateOfBirth;
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

    public void UpdatePasswordHash(string passwordHash)
    {
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, 500, nameof(PasswordHash));

        PasswordHash = passwordHash;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
