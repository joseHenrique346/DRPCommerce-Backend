using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;
using System.Data;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class ProviderAuthorizationStore(StoreCommerceDbContext context) : IProviderAuthorizationStore
{
    public Task<EnterpriseMembership?> FindMembershipAsync(long enterpriseId, long userId, long membershipId, CancellationToken ct)
    {
        return context.EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking()
            .Include(Membership => Membership.User).Include(Membership => Membership.Enterprise)
            .SingleOrDefaultAsync(Membership => Membership.Id == membershipId && Membership.EnterpriseId == enterpriseId &&
                Membership.UserId == userId && Membership.IsActive, ct);
    }

    public Task<bool> HasSettingsPermissionAsync(long enterpriseId, long membershipId, CancellationToken ct)
    {
        return context.MembershipAccessRoles.IgnoreQueryFilters().AnyAsync(Assignment =>
            Assignment.EnterpriseId == enterpriseId && Assignment.EnterpriseMembershipId == membershipId && Assignment.AccessRole.IsActive &&
            Assignment.AccessRole.ListAccessRolePermission.Any(Grant => Grant.Permission.IsActive &&
                Grant.Permission.Code == PermissionCodes.SettingsManage), ct);
    }

    public Task<ProviderAuthorization?> FindAuthorizationAsync(string hash, string browserHash, DateTime now, CancellationToken ct)
    {
        return context.Set<ProviderAuthorization>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Authorization => Authorization.StateHash == hash && Authorization.BrowserHash == browserHash &&
                Authorization.ConsumedAt == null && Authorization.ExpiresAt > now, ct);
    }

    public Task<int> ConsumeAuthorizationAsync(long authorizationId, DateTime now, CancellationToken ct)
    {
        return context.Set<ProviderAuthorization>().IgnoreQueryFilters()
            .Where(Authorization => Authorization.Id == authorizationId && Authorization.ConsumedAt == null && Authorization.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Authorization => Authorization.ConsumedAt, now)
                .SetProperty(Authorization => Authorization.ProtectedVerifier, string.Empty), ct);
    }

    public void Add(ProviderAuthorization authorization)
    {
        context.Add(authorization);
    }

    public async Task<IIdentityTransaction> BeginSerializableTransactionAsync(CancellationToken ct)
    {
        var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        return new IdentityTransaction(transaction);
    }
}
