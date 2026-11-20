using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts;

public interface IEnterpriseAccessReader
{
    Task<IReadOnlyList<EnterpriseMembership>> MembersAsync(long tenant, CancellationToken ct);
    Task<IReadOnlyList<EnterpriseDomain>> DomainsAsync(long tenant, CancellationToken ct);
}
