namespace StoreCommerce.Domain.Interfaces;

public interface ICategoryTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
