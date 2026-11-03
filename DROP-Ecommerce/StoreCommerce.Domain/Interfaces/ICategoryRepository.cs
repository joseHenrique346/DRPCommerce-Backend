using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ICategoryTransaction> BeginMutationAsync(long enterpriseId, CancellationToken cancellationToken);
    Task<(List<Category> Items, int TotalCount)> SearchAsync(
        string? name, long? parentId, bool? isActive, bool rootsOnly,
        int page, int pageSize, CancellationToken cancellationToken);
}
