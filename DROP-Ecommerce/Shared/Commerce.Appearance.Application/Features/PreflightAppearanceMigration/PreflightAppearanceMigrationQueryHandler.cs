using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class PreflightAppearanceMigrationQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    AppearanceSettingsValidator settingsValidator,
    AppearanceResourceValidator resourceValidator,
    AppearanceMigration migration,
    TimeProvider clock) : IRequestHandler<PreflightAppearanceMigrationQuery, AppearanceMigrationPreflightResponse>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearanceMigrationPreflightResponse> Handle(PreflightAppearanceMigrationQuery request, CancellationToken ct)
    {
        var target = request.Target;

        context.RequireAdministrator();
        await resources.EnsureTargetAsync(context.EnterpriseId, target, false, ct);
        var appearance = await repository.GetAsync(context.EnterpriseId, target, ct);

        if (appearance?.PublishedRevisionId is not long currentId)
            return new(false, null, null, null, true, null);

        var current = await repository.GetRevisionAsync(context.EnterpriseId, currentId, ct) ?? throw NotFound();
        var release = await repository.GetReleaseAsync(current.ThemeReleaseId, ct);

        if (release.ReplacementReleaseId is not long replacementId)
            return new(false, release.Version, null, null, true, null);

        var replacement = await repository.GetReleaseAsync(replacementId, ct);

        try
        {
            var settings = migration.Apply(current.SettingsJson, replacement.DefaultsJson, release.MigrationJson);
            await ValidateAsync(context.EnterpriseId, target, replacement, settings, ct);

            return new(true, release.Version, replacement.Version, release.MigrationDeadline, true, null);
        }
        catch (AppearanceFailure failure)
        {
            return new(true, release.Version, replacement.Version, release.MigrationDeadline, false, failure.Code);
        }
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
