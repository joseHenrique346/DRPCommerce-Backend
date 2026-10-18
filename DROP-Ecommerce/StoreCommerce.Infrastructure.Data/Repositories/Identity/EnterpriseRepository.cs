using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EnterpriseRepository(StoreCommerceDbContext context) : Repository<Enterprise>(context), IEnterpriseRepository
{
    public Task LockAsync(long id, CancellationToken cancellationToken)
    {
        return Context.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {id} FOR UPDATE", cancellationToken);
    }
}
