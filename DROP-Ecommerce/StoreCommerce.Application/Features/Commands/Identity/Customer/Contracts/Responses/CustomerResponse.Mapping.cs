using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record CustomerResponse
{
    public static CustomerResponse FromEntity(Customer e) => new(e.Id, e.EnterpriseId, e.FullName, e.Email.Value, e.Phone.Value,
        e.NormalizedEmail, e.EmailConfirmed, e.AddressLine, e.City, e.StateId, e.ZipCode, e.Country, e.Gender,
        e.DateOfBirth, e.IsVerified, e.IsActive, e.IsDeleted, e.DeletedAt, e.CreatedAt, e.UpdatedAt);
}
