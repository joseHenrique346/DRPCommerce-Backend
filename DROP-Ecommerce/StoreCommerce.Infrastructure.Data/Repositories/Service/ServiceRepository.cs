using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity.Service;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(StoreCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<IEnumerable<Service>> GetByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CategoryId == categoryId).ToListAsync(cancellationToken);
    }

    #endregion

    #endregion
}
