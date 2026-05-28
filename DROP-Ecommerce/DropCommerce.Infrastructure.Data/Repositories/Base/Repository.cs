using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DropCommerceDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DropCommerceDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Queries

    #region Get

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
    }

    #endregion

    #region Add

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    #endregion

    #region Update

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    #endregion

    #region Delete

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is null) return;

        if (entity is ISoftDeletable softDeletable)
            softDeletable.SoftDelete();
        else
            _dbSet.Remove(entity);
    }

    public async Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
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
