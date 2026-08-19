using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetListByListIdAsync(List<long> ids, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}
