using StoreCommerce.Application.Interfaces.StoreManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Options;
using SharedCommerce.Application.Security;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class EnterpriseDomainAccess(StoreCommerceDbContext db, IOptions<SecurityOptions> options) : IEnterpriseDomainAccess
{
    public Task<long?> ResolveAsync(string host, long? enterpriseId, CancellationToken ct)
    {
        var normalized = HostName.Normalize(host);
        var platform = DomainHostPolicy.IsPlatformHost(normalized, options.Value.StorefrontBaseDomain);

        return db.EnterpriseDomains.IgnoreQueryFilters().AsNoTracking().Where(Domain =>
                Domain.NormalizedHost == normalized && Domain.IsVerified && Domain.IsActive &&
                !Domain.IsContractBlocked && Domain.Enterprise.IsActive &&
                (!enterpriseId.HasValue || Domain.EnterpriseId == enterpriseId.Value) &&
                (platform || db.EnterprisePlans.Any(Plan =>
                    Plan.Id == Domain.Enterprise.PlanId && Plan.AllowsCustomDomain)))
            .Select(Domain => (long?)Domain.EnterpriseId).SingleOrDefaultAsync(ct);
    }
}
