using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;
using AppearanceEntity = Commerce.Appearance.Domain.Appearance;

namespace Commerce.Appearance.Application.Features;

public sealed class MigrateAppearanceCommandHandler(
    IAppearanceRepository repository,
    IAppearanceResources resources,
    AppearanceSettingsValidator settingsValidator,
    AppearanceResourceValidator resourceValidator,
    AppearanceMigration migration,
    TimeProvider clock) : IRequestHandler<MigrateAppearanceCommand>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task Handle(MigrateAppearanceCommand request, CancellationToken ct)
    {
        var tenant = request.EnterpriseId;
        var target = request.Target;

        string? failure = null;
        long? failedRevisionId = null;

        await using (var transaction = await repository.BeginTransactionAsync(tenant, ct))
        {
            var appearance = await repository.GetAsync(tenant, target, ct);

            if (appearance?.PublishedRevisionId is not long currentId)
                return;

            var current = await repository.GetRevisionAsync(tenant, currentId, ct) ?? throw NotFound();
            var release = await repository.GetReleaseAsync(current.ThemeReleaseId, ct);

            if (release.MigrationDeadline is null || release.MigrationDeadline > Now || release.ReplacementReleaseId is null)
                return;

            try
            {
                var replacement = await repository.GetReleaseAsync(release.ReplacementReleaseId.Value, ct);
                var settings = migration.Apply(current.SettingsJson, replacement.DefaultsJson, release.MigrationJson);
                await resources.EnsureTargetAsync(tenant, target, false, ct);
                await CreateRevisionAsync(appearance, target, replacement, settings, 0, current.Id, AppearanceRevisionOrigin.Migration, ct);
                var session = await repository.GetOpenSessionAsync(tenant, appearance.Id, ct);
                session?.Close(AppearanceEditSessionStatus.Migrated);
                await repository.SaveAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (AppearanceFailure exception)
            {
                failure = exception.Code;
                failedRevisionId = current.Id;
            }
        }

        if (failure is null)
            return;

        await using var failureTransaction = await repository.BeginTransactionAsync(tenant, ct);
        var blocked = await repository.GetAsync(tenant, target, ct) ?? throw NotFound();

        if (blocked.PublishedRevisionId != failedRevisionId)
            return;

        blocked.BlockMigration(failure);
        await repository.SaveAsync(ct);
        await failureTransaction.CommitAsync(ct);
    }

    private async Task<AppearanceRevision> CreateRevisionAsync(
        AppearanceEntity appearance, InputAppearanceTarget target, ThemeRelease release, string settingsJson,
        long editorId, long? restoredFrom, AppearanceRevisionOrigin origin, CancellationToken ct)
    {
        var settings = await ValidateAsync(appearance.EnterpriseId, target, release, settingsJson, ct);
        var number = await repository.NextRevisionNumberAsync(appearance.EnterpriseId, appearance.Id, ct);
        var revision = AppearanceRevision.Create(appearance.EnterpriseId, appearance.Id, release.Id,
            number, settings, ConfigurationChecksum(release, settings), editorId, Now, restoredFrom, origin);
        repository.Add(revision);
        await repository.SaveAsync(ct);
        appearance.Publish(revision.Id);

        return revision;
    }

    private async Task<string> ValidateAsync(long tenant, InputAppearanceTarget target, ThemeRelease release, string settingsJson, CancellationToken ct)
    {
        EnsureReleaseAvailable(release);
        var settings = settingsValidator.Validate(release.SchemaJson, settingsJson);
        await resourceValidator.ValidateAsync(tenant, target, release.SchemaJson, settings, ct);

        return settings;
    }

    private void EnsureReleaseAvailable(ThemeRelease release)
    {
        if (release.AppearanceChannelId != repository.Channel.Id || release.RendererContract != 1 || release.MigrationDeadline <= Now)
            throw new AppearanceFailure(409, "Appearance.ReleaseUnavailable", "A versão do tema não está disponível para novas publicações.");
    }

    private static AppearanceFailure NotFound()
    {
        return new AppearanceFailure(404, "Appearance.NotFound", "Aparência ou revisão indisponível.");
    }
}
