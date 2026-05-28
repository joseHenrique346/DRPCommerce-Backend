using StoreCommerce.Domain.Entity.Category;

namespace StoreCommerce.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
