namespace StoreCommerce.Domain.Entity;

public class Customer : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public string FullName { get; private set; }
    public string PasswordHash { get; private set; }
    public string AddressLine { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public string Gender { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }

    public Customer() { }

    public Customer(long enterpriseId, string fullName, string passwordHash, string addressLine, string city, string state, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive)
    {
        EnterpriseId = enterpriseId;
        FullName = fullName;
        PasswordHash = passwordHash;
        AddressLine = addressLine;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        IsVerified = isVerified;
        IsActive = isActive;
    }
}