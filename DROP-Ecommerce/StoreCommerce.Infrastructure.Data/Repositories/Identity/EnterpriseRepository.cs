using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EnterpriseRepository(StoreCommerceDbContext context) : Repository<Enterprise>(context), IEnterpriseRepository
{
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
