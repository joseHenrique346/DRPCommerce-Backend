namespace StoreCommerce.Domain.Interfaces.Persistence;

public interface IStoreTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
