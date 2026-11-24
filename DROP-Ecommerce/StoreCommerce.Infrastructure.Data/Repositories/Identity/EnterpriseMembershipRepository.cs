using StoreCommerce.Domain.Interfaces.StoreManagement;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EnterpriseMembershipRepository(StoreCommerceDbContext context) : Repository<EnterpriseMembership>(context), IEnterpriseMembershipRepository
{
    public async Task<List<EnterpriseMembership>> GetActiveForUserAsync(long userId, CancellationToken cancellationToken)
    {
        var candidates = await Context.EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking()
            .Include(Membership => Membership.Enterprise)
            .Where(Membership => Membership.UserId == userId && Membership.IsActive &&
                !Membership.IsContractBlocked && Membership.Enterprise.IsActive)
            .OrderBy(Membership => Membership.Enterprise.TradeName)
            .ToListAsync(cancellationToken);
        var available = new List<EnterpriseMembership>();

        foreach (var membership in candidates)
        {
            var rights = StoreCommerce.Domain.Contracts.EnterpriseEntitlements.Resolve(
                membership.Enterprise.PlanId, membership.Enterprise.AddOnId);

            if (membership.KindId != 1 && await Context.EnterpriseMemberships.IgnoreQueryFilters().CountAsync(Membership =>
                Membership.EnterpriseId == membership.EnterpriseId && Membership.IsActive &&
                !Membership.IsContractBlocked && Membership.KindId != 1 && Membership.User.IsActive, cancellationToken) > rights.EmployeeLimit)
                continue;

            available.Add(membership);
        }

        return available;
    }

    public Task<List<EnterpriseMembership>> GetWithRoleAsync(long roleId, long enterpriseId, CancellationToken cancellationToken)
    {
        return Context.EnterpriseMemberships.IgnoreQueryFilters()
            .Where(Membership => Membership.EnterpriseId == enterpriseId &&
                Context.MembershipAccessRoles.IgnoreQueryFilters().Any(Role =>
                    Role.EnterpriseMembershipId == Membership.Id && Role.AccessRoleId == roleId && Role.EnterpriseId == enterpriseId))
            .ToListAsync(cancellationToken);
    }

    public Task<List<string>> GetPermissionsAsync(long membershipId, long enterpriseId, CancellationToken cancellationToken)
    {
        return Context.MembershipAccessRoles.IgnoreQueryFilters()
            .Where(Role => Role.EnterpriseMembershipId == membershipId && Role.EnterpriseId == enterpriseId && Role.AccessRole.IsActive)
            .SelectMany(Role => Role.AccessRole.ListAccessRolePermission)
            .Where(Permission => Permission.EnterpriseId == enterpriseId && Permission.Permission.IsActive)
            .Select(Permission => Permission.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public Task<long?> GetEmployeeIdAsync(long membershipId, CancellationToken cancellationToken)
    {
        return Context.Employees.IgnoreQueryFilters()
            .Where(Employee => Employee.EnterpriseMembershipId == membershipId && Employee.IsActive)
            .Select(Employee => (long?)Employee.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
