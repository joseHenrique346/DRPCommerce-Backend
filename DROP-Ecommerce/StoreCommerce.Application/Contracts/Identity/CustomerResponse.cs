namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record CustomerResponse(
    long Id, long EnterpriseId, string FullName, string Email, string Phone,
    string NormalizedEmail, bool EmailConfirmed, string AddressLine, string City,
    long StateId, string ZipCode, string Country, string Gender, DateTime DateOfBirth,
    bool IsVerified, bool IsActive, bool IsDeleted, DateTime? DeletedAt,
    DateTime CreatedAt, DateTime? UpdatedAt)
{
    public long CustomerId => Id;
}
