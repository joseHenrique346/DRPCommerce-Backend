using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class IdentityRepositories(
    StoreCommerceDbContext context,
    IUserRepository users,
    ICustomerRepository customers,
    IEnterpriseRepository enterprises,
    IEnterpriseMembershipRepository enterpriseMemberships,
    IEnterpriseDomainRepository enterpriseDomains,
    IAccessRoleRepository accessRoles,
    IPermissionRepository permissions,
    IAccessRolePermissionRepository accessRolePermissions,
    IMembershipAccessRoleRepository membershipAccessRoles,
    IEntityRepository<UserRefreshToken> userRefreshTokens,
    IEntityRepository<CustomerRefreshToken> customerRefreshTokens,
    IEntityRepository<UserActionToken> userActionTokens,
    IEntityRepository<CustomerActionToken> customerActionTokens,
    IEntityRepository<EnterpriseInvitation> enterpriseInvitations,
    IEntityRepository<InvitationAccessRole> invitationAccessRoles,
    IEntityRepository<OwnershipTransfer> ownershipTransfers,
    IEntityRepository<StoreSettings> storeSettings,
    IAuthenticationSessionRepository sessions) : IIdentityRepositories
{
    public IUserRepository Users { get; } = users;
    public ICustomerRepository Customers { get; } = customers;
    public IEnterpriseRepository Enterprises { get; } = enterprises;
    public IEnterpriseMembershipRepository EnterpriseMemberships { get; } = enterpriseMemberships;
    public IEnterpriseDomainRepository EnterpriseDomains { get; } = enterpriseDomains;
    public IAccessRoleRepository AccessRoles { get; } = accessRoles;
    public IPermissionRepository Permissions { get; } = permissions;
    public IAccessRolePermissionRepository AccessRolePermissions { get; } = accessRolePermissions;
    public IMembershipAccessRoleRepository MembershipAccessRoles { get; } = membershipAccessRoles;
    public IEntityRepository<UserRefreshToken> UserRefreshTokens { get; } = userRefreshTokens;
    public IEntityRepository<CustomerRefreshToken> CustomerRefreshTokens { get; } = customerRefreshTokens;
    public IEntityRepository<UserActionToken> UserActionTokens { get; } = userActionTokens;
    public IEntityRepository<CustomerActionToken> CustomerActionTokens { get; } = customerActionTokens;
    public IEntityRepository<EnterpriseInvitation> EnterpriseInvitations { get; } = enterpriseInvitations;
    public IEntityRepository<InvitationAccessRole> InvitationAccessRoles { get; } = invitationAccessRoles;
    public IEntityRepository<OwnershipTransfer> OwnershipTransfers { get; } = ownershipTransfers;
    public IEntityRepository<StoreSettings> StoreSettings { get; } = storeSettings;
    public IAuthenticationSessionRepository Sessions { get; } = sessions;

    public Task<EnterpriseInvitation> GetInvitationWithRolesAsync(long id, CancellationToken cancellationToken)
    {
        return context.EnterpriseInvitations.IgnoreQueryFilters()
            .Include(Invitation => Invitation.AccessRoles)
            .SingleAsync(Invitation => Invitation.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MembershipChoice>> GetMembershipChoicesAsync(long userId, CancellationToken cancellationToken)
    {
        return await context.EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking()
            .Where(Membership => Membership.UserId == userId && Membership.IsActive)
            .OrderBy(Membership => Membership.Enterprise.TradeName)
            .Select(Membership => new MembershipChoice(Membership.Id, Membership.EnterpriseId, Membership.Enterprise.TradeName, Membership.KindId))
            .ToListAsync(cancellationToken);
    }
}
