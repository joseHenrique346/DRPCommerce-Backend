namespace DropCommerce.Domain.Interfaces;

public interface IDropTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
