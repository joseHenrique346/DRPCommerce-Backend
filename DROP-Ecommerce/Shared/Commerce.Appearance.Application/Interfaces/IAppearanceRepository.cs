using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using AppearanceEntity = Commerce.Appearance.Domain.Appearance;

namespace Commerce.Appearance.Application;

public interface IAppearanceRepository
{
    AppearanceChannel Channel { get; }
    Task<IAppearanceTransaction> BeginTransactionAsync(long enterpriseId, CancellationToken ct);
    Task<AppearanceEntity?> GetAsync(long enterpriseId, InputAppearanceTarget target, CancellationToken ct);
    Task<ThemeRelease?> GetReleaseAsync(string key, string version, CancellationToken ct);
    Task<ThemeRelease> GetReleaseAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<ThemeRelease>> GetReleasesAsync(CancellationToken ct);
    Task<AppearanceEditSession?> GetOpenSessionAsync(long enterpriseId, long appearanceId, CancellationToken ct);
    Task<AppearanceEditSession?> GetSessionAsync(long enterpriseId, long id, CancellationToken ct);
    Task<AppearanceRevision?> GetRevisionAsync(long enterpriseId, long id, CancellationToken ct);
    Task<IReadOnlyList<AppearanceRevision>> GetRevisionsAsync(long enterpriseId, long appearanceId, CancellationToken ct, long? beforeNumber = null);
    Task<long> NextRevisionNumberAsync(long enterpriseId, long appearanceId, CancellationToken ct);
    void Add(AppearanceEntity appearance);
    void Add(AppearanceEditSession session);
    void Add(AppearanceRevision revision);
    void Add(ThemeRelease release);
    Task SaveAsync(CancellationToken ct);
    Task<IReadOnlyList<(long EnterpriseId, InputAppearanceTarget Target)>> GetMigrationTargetsAsync(DateTime now, CancellationToken ct);
}

public interface IAppearanceTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
