using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Identity;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IIdentityTransaction? CurrentTransaction { get; }
    new Task<IIdentityTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
