using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public partial class Customer : BaseEntity, ISoftDeletable, ITenantEntity
{
    #region Constants
    public const int FullNameMaxLength = 200;
    public const int PasswordHashMaxLength = 500;
    public const int NormalizedEmailMaxLength = 255;
    public const int AddressLineMaxLength = 200;
    public const int CityMaxLength = 100;
    public const int ZipCodeMaxLength = 20;
    public const int CountryMaxLength = 100;
    public const int GenderMaxLength = 100;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public string FullName { get; private set; }
    public CustomerEmail Email { get; private set; }
    public CustomerPhone Phone { get; private set; }
    [System.Text.Json.Serialization.JsonIgnore] public string? PasswordHash { get; private set; }
    public string NormalizedEmail { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public long SecurityVersion { get; private set; } = 1;
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

    private Customer(long enterpriseId, string fullName, CustomerEmail email, CustomerPhone phone, string? passwordHash, string addressLine, string city, long stateId, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        EnterpriseId = enterpriseId;
        FullName = fullName;
        Email = email;
        NormalizedEmail = email.Value.Trim().ToUpperInvariant();
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
    public static Customer Create(long enterpriseId, string fullName, CustomerEmail email, CustomerPhone phone, string addressLine, string city, long stateId, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, FullNameMaxLength, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));
        BaseValidate.ValidateMaxLength(addressLine, AddressLineMaxLength, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, CityMaxLength, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, ZipCodeMaxLength, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, CountryMaxLength, nameof(Country));
        BaseValidate.ValidateMaxLength(gender, GenderMaxLength, nameof(Gender));
        BaseValidate.ValidateNotFuture(dateOfBirth, nameof(DateOfBirth));

        return new Customer(enterpriseId, fullName, email, phone, null, addressLine, city, stateId, zipCode, country, gender, dateOfBirth, isVerified, isActive);
    }

    public void UpdatePersonalInfo(string fullName, CustomerEmail email, CustomerPhone phone, string gender, DateTime dateOfBirth)
    {
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, FullNameMaxLength, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNull(phone, nameof(Phone));
        BaseValidate.ValidateMaxLength(gender, GenderMaxLength, nameof(Gender));
        BaseValidate.ValidateNotFuture(dateOfBirth, nameof(DateOfBirth));

        FullName = fullName;
        if (NormalizedEmail != email.Value.Trim().ToUpperInvariant())
        {
            EmailConfirmed = false;
            InvalidateSessions();
        }
        Email = email;
        NormalizedEmail = email.Value.Trim().ToUpperInvariant();
        Phone = phone;
        Gender = gender;
        DateOfBirth = dateOfBirth;
    }

    public void UpdateAddress(string addressLine, string city, long stateId, string zipCode, string country)
    {
        BaseValidate.ValidateMaxLength(addressLine, AddressLineMaxLength, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, CityMaxLength, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));
        BaseValidate.ValidateMaxLength(zipCode, ZipCodeMaxLength, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, CountryMaxLength, nameof(Country));

        AddressLine = addressLine;
        City = city;
        StateId = stateId;
        ZipCode = zipCode;
        Country = country;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, PasswordHashMaxLength, nameof(PasswordHash));

        PasswordHash = passwordHash;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    #endregion
}
