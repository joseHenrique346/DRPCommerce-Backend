using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class EnterpriseAccessReader(StoreCommerceDbContext db) : IEnterpriseAccessReader
{
    public async Task<IReadOnlyList<EnterpriseMembership>> MembersAsync(long tenant, CancellationToken ct)
    {
        return await db.EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking().Include(Membership => Membership.User)
            .Where(Membership => Membership.EnterpriseId == tenant).OrderBy(Membership => Membership.JoinedAt)
            .ThenBy(Membership => Membership.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnterpriseDomain>> DomainsAsync(long tenant, CancellationToken ct)
    {
        return await db.EnterpriseDomains.IgnoreQueryFilters().AsNoTracking()
            .Where(Domain => Domain.EnterpriseId == tenant).OrderBy(Domain => Domain.Id).ToListAsync(ct);
    }
}
