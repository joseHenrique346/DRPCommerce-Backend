using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IEnterpriseMembershipRepository : IRepository<EnterpriseMembership>
{
    Task<List<EnterpriseMembership>> GetActiveForUserAsync(long userId, CancellationToken cancellationToken);
    Task<List<EnterpriseMembership>> GetWithRoleAsync(long roleId, long enterpriseId, CancellationToken cancellationToken);
    Task<List<string>> GetPermissionsAsync(long membershipId, long enterpriseId, CancellationToken cancellationToken);
    Task<long?> GetEmployeeIdAsync(long membershipId, CancellationToken cancellationToken);
}
