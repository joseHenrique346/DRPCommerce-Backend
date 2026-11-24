namespace SharedCommerce.Application.Interfaces.Appearance;

public interface IAppearanceTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
