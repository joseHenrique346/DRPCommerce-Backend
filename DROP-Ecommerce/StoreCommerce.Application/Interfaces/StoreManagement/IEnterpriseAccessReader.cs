using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.StoreManagement;

public interface IEnterpriseAccessReader
{
    Task<IReadOnlyList<EnterpriseMembership>> MembersAsync(long tenant, CancellationToken ct);
    Task<IReadOnlyList<EnterpriseDomain>> DomainsAsync(long tenant, CancellationToken ct);
}
