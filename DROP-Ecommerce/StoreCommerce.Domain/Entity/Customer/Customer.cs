using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Customer : BaseEntity, ISoftDeletable, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string FullName { get; private set; }
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
    #endregion

    #region Constructor
    protected Customer() { }

    private Customer(long enterpriseId, string fullName, string passwordHash, string addressLine, string city, string state, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        EnterpriseId = enterpriseId;
        FullName = fullName;
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
    public static Customer Create(long enterpriseId, string fullName, string passwordHash, string addressLine, string city, string state, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, 500, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(addressLine, 500, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(city, 255, nameof(City));
        BaseValidate.ValidateMaxLength(state, 255, nameof(State));
        BaseValidate.ValidateMaxLength(zipCode, 20, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(country, 100, nameof(Country));
        BaseValidate.ValidateMaxLength(gender, 50, nameof(Gender));
        BaseValidate.ValidateNotFuture(dateOfBirth, nameof(DateOfBirth));

        return new Customer(enterpriseId, fullName, passwordHash, addressLine, city, state, zipCode, country, gender, dateOfBirth, isVerified, isActive);
    }

    public void UpdatePersonalInfo(string fullName, string gender, DateTime dateOfBirth)
    {
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateMaxLength(gender, 50, nameof(Gender));
        BaseValidate.ValidateNotFuture(dateOfBirth, nameof(DateOfBirth));

        FullName = fullName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
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

    public void UpdatePasswordHash(string passwordHash)
    {
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, 500, nameof(PasswordHash));

        PasswordHash = passwordHash;
    }
    #endregion
}
