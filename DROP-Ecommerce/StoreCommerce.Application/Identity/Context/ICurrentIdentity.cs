namespace StoreCommerce.Application.Identity;

public interface ICurrentIdentity
{
    long UserId { get; }
    long CustomerId { get; }
    long EnterpriseId { get; }
    long MembershipId { get; }
    Guid FamilyId { get; }
    string Host { get; }
    void RequirePrimaryOwner();
}
