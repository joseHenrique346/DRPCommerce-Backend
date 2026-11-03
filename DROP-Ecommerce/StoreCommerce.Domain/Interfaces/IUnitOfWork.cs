namespace StoreCommerce.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task<IStoreTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
