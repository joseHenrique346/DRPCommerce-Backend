using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Interfaces.Identity;

public interface ICurrentIdentity
{
    long UserId { get; }
    long CustomerId { get; }
    long EnterpriseId { get; }
    long MembershipId { get; }
    Guid FamilyId { get; }
    string? StorefrontSlug { get; }
    void RequirePrimaryOwner();
    void RequirePermission(string permission);
}
