using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces.Identity;

public interface IUserRepository : IRepository<User>
{
    Task LockAsync(long id, CancellationToken cancellationToken);
}
