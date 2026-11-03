using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(StoreCommerceDbContext context) : base(context)
    {
    }

    public async Task<ICategoryTransaction> BeginMutationAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Serializa alterações da árvore e dos slugs dentro da loja.
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT id FROM empresa WHERE id = {enterpriseId} FOR UPDATE", cancellationToken);

            return new CategoryTransaction(transaction);
        }
        catch
        {
            await transaction.DisposeAsync();

            throw;
        }
    }

    public async Task<(List<Category> Items, int TotalCount)> SearchAsync(
        string? name, long? parentId, bool? isActive, bool rootsOnly,
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = Context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim().ToLowerInvariant();
            query = query.Where(Category => Category.Name.ToLower().Contains(term));
        }

        if (parentId.HasValue)
            query = query.Where(Category => Category.ParentCategoryId == parentId);

        if (rootsOnly)
            query = query.Where(Category => Category.ParentCategoryId == null);

        if (isActive.HasValue)
            query = query.Where(Category => Category.IsActive == isActive);

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(Category => Category.DisplayOrder)
            .ThenBy(Category => Category.Name)
            .ThenBy(Category => Category.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, count);
    }

    private sealed class CategoryTransaction(IDbContextTransaction transaction) : ICategoryTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken)
        {
            return transaction.CommitAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return transaction.DisposeAsync();
        }
    }

    #region Queries

    #region Get

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Slug == slug, cancellationToken);
    }

    #endregion

    #endregion
}
