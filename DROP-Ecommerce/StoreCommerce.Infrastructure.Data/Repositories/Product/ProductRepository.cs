using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(StoreCommerceDbContext context) : base(context) { }

    #region Queries

    #region Get

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => e.CategoryId == categoryId).ToListAsync(cancellationToken);
    }

    public Task<List<Product>> GetStorefrontAsync(IReadOnlyCollection<long> categoryIds, int page, int pageSize, CancellationToken cancellationToken)
    {
        return Context.Products.AsNoTracking()
            .Where(Product => Product.IsActive && categoryIds.Contains(Product.CategoryId))
            .OrderBy(Product => Product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #endregion
}
