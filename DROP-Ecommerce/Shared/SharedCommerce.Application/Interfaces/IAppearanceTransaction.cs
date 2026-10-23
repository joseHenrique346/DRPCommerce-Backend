namespace SharedCommerce.Application.Interfaces;

public interface IAppearanceTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
