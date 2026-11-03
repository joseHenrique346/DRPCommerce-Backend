using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Identity;

public interface IIdentityRepositories
{
    IUserRepository Users { get; }
    ICustomerRepository Customers { get; }
    IEnterpriseRepository Enterprises { get; }
    IEnterpriseMembershipRepository EnterpriseMemberships { get; }
    IEnterpriseDomainRepository EnterpriseDomains { get; }
    IAccessRoleRepository AccessRoles { get; }
    IPermissionRepository Permissions { get; }
    IAccessRolePermissionRepository AccessRolePermissions { get; }
    IMembershipAccessRoleRepository MembershipAccessRoles { get; }
    IEntityRepository<UserRefreshToken> UserRefreshTokens { get; }
    IEntityRepository<CustomerRefreshToken> CustomerRefreshTokens { get; }
    IEntityRepository<UserActionToken> UserActionTokens { get; }
    IEntityRepository<CustomerActionToken> CustomerActionTokens { get; }
    IEntityRepository<EnterpriseInvitation> EnterpriseInvitations { get; }
    IEntityRepository<InvitationAccessRole> InvitationAccessRoles { get; }
    IEntityRepository<OwnershipTransfer> OwnershipTransfers { get; }
    IEntityRepository<StoreSettings> StoreSettings { get; }
    IAuthenticationSessionRepository Sessions { get; }
    Task<EnterpriseInvitation> GetInvitationWithRolesAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MembershipChoice>> GetMembershipChoicesAsync(long userId, CancellationToken cancellationToken);
}
