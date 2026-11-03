using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Domain.Models;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(StoreCommerceDbContext context) : base(context)
    {
    }

    private IQueryable<Product> WithVariants()
    {
        return Context.Products.Include(Product => Product.ListProductVariant).AsSplitQuery();
    }

    public Task LockCatalogAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        return Context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT id FROM empresa WHERE id = {enterpriseId} FOR UPDATE", cancellationToken);
    }

    public override Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return WithVariants().SingleOrDefaultAsync(Product => Product.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await WithVariants().AsNoTracking().ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<Product>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken = default)
    {
        return await GetWithVariantsAsync(ids, cancellationToken);
    }

    public Task<List<Product>> GetWithVariantsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken)
    {
        return WithVariants().Where(Product => ids.Contains(Product.Id)).ToListAsync(cancellationToken);
    }

    public Task<Product?> GetWithVariantsBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return WithVariants().AsNoTracking().SingleOrDefaultAsync(Product => Product.Slug == slug, cancellationToken);
    }

    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var query = WithVariants().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Term))
        {
            var term = criteria.Term.Trim().ToLowerInvariant();
            query = query.Where(Product => Product.Name.ToLower().Contains(term) || Product.SKU.ToLower().Contains(term) ||
                Product.Brand.ToLower().Contains(term) || Product.ListProductVariant.Any(Variant => !Variant.IsDeleted && Variant.SKU.ToLower().Contains(term)));
        }

        if (criteria.CategoryIds is not null)
            query = query.Where(Product => criteria.CategoryIds.Contains(Product.CategoryId));

        if (!string.IsNullOrWhiteSpace(criteria.Brand))
        {
            var brand = criteria.Brand.Trim().ToLowerInvariant();
            query = query.Where(Product => Product.Brand.ToLower() == brand);
        }

        if (criteria.Storefront)
            query = query.Where(Product => Product.IsActive);
        else if (criteria.IsActive.HasValue)
            query = query.Where(Product => Product.IsActive == criteria.IsActive);

        if (criteria.Storefront || criteria.MinPrice.HasValue || criteria.MaxPrice.HasValue || criteria.InStock.HasValue)
            query = query.Where(Product => Product.ListProductVariant.Any(Variant => !Variant.IsDeleted && Variant.IsActive &&
                (!Product.HasVariants || !Variant.IsDefault) &&
                (!criteria.MinPrice.HasValue || (Variant.Price ?? Product.Price) >= criteria.MinPrice) &&
                (!criteria.MaxPrice.HasValue || (Variant.Price ?? Product.Price) <= criteria.MaxPrice) &&
                (!criteria.InStock.HasValue || (Product.IsDigital || Variant.StockQuantity > Variant.ReservedQuantity) == criteria.InStock)));

        var count = await query.CountAsync(cancellationToken);
        var ordered = criteria.Sort switch
        {
            "price_asc" => query.OrderBy(Product => Product.ListProductVariant
                .Where(Variant => !Variant.IsDeleted && Variant.IsActive && (!Product.HasVariants || !Variant.IsDefault))
                .Select(Variant => (decimal?)(Variant.Price ?? Product.Price)).Min() ?? Product.Price),
            "price_desc" => query.OrderByDescending(Product => Product.ListProductVariant
                .Where(Variant => !Variant.IsDeleted && Variant.IsActive && (!Product.HasVariants || !Variant.IsDefault))
                .Select(Variant => (decimal?)(Variant.Price ?? Product.Price)).Min() ?? Product.Price),
            "newest" => query.OrderByDescending(Product => Product.Id),
            _ => query.OrderBy(Product => Product.Name)
        };
        var items = await ordered.ThenBy(Product => Product.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize).ToListAsync(cancellationToken);

        return (items, count);
    }

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

    public async Task<List<Product>> GetStorefrontAsync(IReadOnlyCollection<long> categoryIds, int page, int pageSize, CancellationToken cancellationToken)
    {
        var result = await SearchAsync(new ProductSearchCriteria(null, categoryIds, null, null, null, null, null,
            "newest", page, pageSize, true), cancellationToken);

        return result.Items;
    }

    #endregion

    #endregion
}
