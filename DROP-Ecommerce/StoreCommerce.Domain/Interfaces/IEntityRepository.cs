using System.Linq.Expressions;

namespace StoreCommerce.Domain.Interfaces;

public interface IEntityRepository<TEntity> where TEntity : class
{
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false, bool tracking = true);
    Task<TEntity> GetRequiredAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false, bool tracking = true);
    Task<TResponse?> FindValueAsync<TResponse>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResponse>> selector, CancellationToken cancellationToken, bool ignoreQueryFilters = false);
    Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false, bool tracking = true);
    Task<List<TResponse>> ListAsync<TResponse>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResponse>> selector, CancellationToken cancellationToken, bool ignoreQueryFilters = false);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false);
    void Add(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}
