using StoreCommerce.Domain.Interfaces.StoreManagement;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EnterpriseRepository(StoreCommerceDbContext context) : Repository<Enterprise>(context), IEnterpriseRepository
{
    public Task<long?> FindActiveStorefrontIdAsync(string slug, CancellationToken ct)
    {
        return Context.Enterprises.IgnoreQueryFilters().AsNoTracking()
            .Where(Enterprise => Enterprise.StorefrontSlug == slug && Enterprise.IsActive)
            .Select(Enterprise => (long?)Enterprise.Id)
            .SingleOrDefaultAsync(ct);
    }

    public Task<EnterpriseContractState?> GetContractStateAsync(long id, CancellationToken cancellationToken)
    {
        return Context.Enterprises.IgnoreQueryFilters().AsNoTracking()
            .Where(Enterprise => Enterprise.Id == id && Enterprise.IsActive)
            .Select(Enterprise => new EnterpriseContractState(Enterprise.Id, Enterprise.PlanId,
                Enterprise.AddOnId, Enterprise.ContractRevision, Enterprise.CatalogVersion))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task LockAsync(long id, CancellationToken cancellationToken)
    {
        return Context.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {id} FOR UPDATE", cancellationToken);
    }
}
