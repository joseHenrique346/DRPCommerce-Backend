using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public abstract class RelationshipRepository<TEntity>(StoreCommerceDbContext context) : EntityRepository<TEntity>(context), IRelationshipRepository<TEntity>
    where TEntity : class
{
    protected DbSet<TEntity> Entities { get; } = context.Set<TEntity>();
    protected abstract Expression<Func<TEntity, bool>> MatchKey(long firstId, long secondId);

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Entities.ToListAsync(cancellationToken);
    }

    public virtual Task<TEntity?> GetByKeyAsync(long firstId, long secondId, CancellationToken cancellationToken = default)
    {
        return Entities.FirstOrDefaultAsync(MatchKey(firstId, secondId), cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    public virtual void Delete(TEntity entity)
    {
        Entities.Remove(entity);
    }
}
