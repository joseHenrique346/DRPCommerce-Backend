namespace StoreCommerce.Domain.Interfaces;

public interface IStoreTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
