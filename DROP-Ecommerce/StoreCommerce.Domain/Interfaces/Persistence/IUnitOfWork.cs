namespace StoreCommerce.Domain.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task<IStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
