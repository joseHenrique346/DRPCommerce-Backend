using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task LockAsync(long id, CancellationToken cancellationToken);
}
