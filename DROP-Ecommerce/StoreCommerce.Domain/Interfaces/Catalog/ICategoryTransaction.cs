namespace StoreCommerce.Domain.Interfaces.Catalog;

public interface ICategoryTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
