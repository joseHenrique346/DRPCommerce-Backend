using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class UserRepository(StoreCommerceDbContext context) : Repository<User>(context), IUserRepository
{
    public Task LockAsync(long id, CancellationToken cancellationToken)
    {
        return Context.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM usuario WHERE id = {id} FOR UPDATE", cancellationToken);
    }
}
