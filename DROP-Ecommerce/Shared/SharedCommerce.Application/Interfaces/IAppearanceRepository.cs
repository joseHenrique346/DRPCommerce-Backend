using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.StaticEntity;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;

namespace SharedCommerce.Application.Interfaces;

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
