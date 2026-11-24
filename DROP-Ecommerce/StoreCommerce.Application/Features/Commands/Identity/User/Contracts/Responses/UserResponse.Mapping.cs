using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record UserResponse
{
    public static UserResponse FromEntity(User e) => new(e.Id, e.FullName, e.Email.Value, e.NormalizedEmail, e.EmailConfirmed, e.IsActive, e.LastLoginAt);
}
