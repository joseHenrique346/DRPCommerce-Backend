using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record UserResponse
{
    public static UserResponse FromEntity(User e) => new(e.Id, e.FullName, e.Email.Value, e.NormalizedEmail, e.EmailConfirmed, e.IsActive, e.LastLoginAt);
}
public sealed partial record CustomerResponse
{
    public static CustomerResponse FromEntity(Customer e) => new(e.Id, e.EnterpriseId, e.FullName, e.Email.Value, e.Phone.Value,
        e.NormalizedEmail, e.EmailConfirmed, e.AddressLine, e.City, e.StateId, e.ZipCode, e.Country, e.Gender,
        e.DateOfBirth, e.IsVerified, e.IsActive, e.IsDeleted, e.DeletedAt, e.CreatedAt, e.UpdatedAt);
}
public sealed partial record EnterpriseMembershipResponse
{
    public static EnterpriseMembershipResponse FromEntity(EnterpriseMembership e) => new(e.Id, e.EnterpriseId, e.UserId, e.KindId, e.IsActive, e.JoinedAt);
}
public sealed partial record EnterpriseDomainResponse
{
    public static EnterpriseDomainResponse FromEntity(EnterpriseDomain e) => new(e.Id, e.EnterpriseId, e.Host, e.NormalizedHost, e.IsPrimary, e.IsVerified, e.IsActive);
}
public sealed partial record AccessRoleResponse
{
    public static AccessRoleResponse FromEntity(AccessRole e) => new(e.Id, e.EnterpriseId, e.Name, e.NormalizedName, e.Description, e.IsActive);
}
