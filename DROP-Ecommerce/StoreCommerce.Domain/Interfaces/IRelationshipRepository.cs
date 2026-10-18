namespace StoreCommerce.Domain.Interfaces;

public interface IRelationshipRepository<TEntity> : IEntityRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetByKeyAsync(long firstId, long secondId, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Delete(TEntity entity);
}
