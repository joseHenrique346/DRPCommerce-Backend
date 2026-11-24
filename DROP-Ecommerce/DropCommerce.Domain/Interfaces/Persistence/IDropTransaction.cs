namespace DropCommerce.Domain.Interfaces.Persistence;

public interface IDropTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
