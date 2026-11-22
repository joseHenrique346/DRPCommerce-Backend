using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Interfaces.Identity;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IIdentityTransaction? CurrentTransaction { get; }
    new Task<IIdentityTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
