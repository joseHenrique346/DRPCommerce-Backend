using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EntityRepository<TEntity>(StoreCommerceDbContext context) : IEntityRepository<TEntity>
    where TEntity : class
{
    protected StoreCommerceDbContext Context { get; } = context;

    private IQueryable<TEntity> Query(bool ignoreQueryFilters, bool tracking)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        if (ignoreQueryFilters)
            query = query.IgnoreQueryFilters();

        if (!tracking)
            query = query.AsNoTracking();

        return query;
    }

    public virtual Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false, bool tracking = true)
    {
        return Query(ignoreQueryFilters, tracking).SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual Task<TEntity> GetRequiredAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false, bool tracking = true)
    {
        return Query(ignoreQueryFilters, tracking).SingleAsync(predicate, cancellationToken);
    }

    public virtual Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false, bool tracking = true)
    {
        return Query(ignoreQueryFilters, tracking).Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual Task<TResponse?> FindValueAsync<TResponse>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResponse>> selector, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters, false).Where(predicate).Select(selector).SingleOrDefaultAsync(cancellationToken);
    }

    public virtual Task<List<TResponse>> ListAsync<TResponse>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResponse>> selector, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters, false).Where(predicate).Select(selector).ToListAsync(cancellationToken);
    }

    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters, false).AnyAsync(predicate, cancellationToken);
    }

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters, false).CountAsync(predicate, cancellationToken);
    }

    public virtual void Add(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        Context.Set<TEntity>().RemoveRange(entities);
    }
}
