using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
    Task<List<Product>> GetStorefrontAsync(IReadOnlyCollection<long> categoryIds, int page, int pageSize, CancellationToken cancellationToken);
    Task LockCatalogAsync(long enterpriseId, CancellationToken cancellationToken);
    Task<List<Product>> GetWithVariantsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken);
    Task<Product?> GetWithVariantsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<(List<Product> Items, int TotalCount)> SearchAsync(Models.ProductSearchCriteria criteria, CancellationToken cancellationToken);
}
