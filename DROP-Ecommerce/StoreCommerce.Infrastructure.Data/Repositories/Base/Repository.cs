using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class Repository<T> : EntityRepository<T>, IRepository<T> where T : BaseEntity
{
    protected readonly StoreCommerceDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(StoreCommerceDbContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Queries

    #region Get

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
    }

    #endregion

    #region Add

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    #endregion

    #region Update

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    #endregion

    #region Delete

    public virtual async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is null)
            return;

        if (entity is ISoftDeletable softDeletable)
            softDeletable.SoftDelete();
        else
            _dbSet.Remove(entity);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            if (entity is ISoftDeletable softDeletable)
                softDeletable.SoftDelete();
            else
                _dbSet.Remove(entity);
        }
    }

    #endregion

    #endregion
}
