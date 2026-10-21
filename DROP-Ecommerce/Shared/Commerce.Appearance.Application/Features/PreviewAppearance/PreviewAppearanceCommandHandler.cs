using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;
using AppearanceEntity = Commerce.Appearance.Domain.Appearance;

namespace Commerce.Appearance.Application.Features;

public sealed class PreviewAppearanceCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    AppearanceSettingsValidator settingsValidator,
    AppearanceResourceValidator resourceValidator,
    TimeProvider clock) : IRequestHandler<PreviewAppearanceCommand, AppearancePreviewResponse>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearancePreviewResponse> Handle(PreviewAppearanceCommand request, CancellationToken ct)
    {
        var target = request.Target;
        var sessionId = request.SessionId;
        var version = request.Version;

        context.RequireAdministrator();
        await using var transaction = await repository.BeginTransactionAsync(context.EnterpriseId, ct);
        var (_, session, release) = await RequireSessionAsync(target, sessionId, version, ct);
        var settings = await ValidateAsync(context.EnterpriseId, target, release, session.SettingsJson, ct);
        var preview = Manifest(repository.Channel, null, release, settings, TargetCode(target), Now);
        session.Preview(System.Text.Json.JsonSerializer.Serialize(preview), Now);
        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);

        return new AppearancePreviewResponse(EditResponse(session, release), preview);
    }

    private async Task<(AppearanceEntity, AppearanceEditSession, ThemeRelease)> RequireSessionAsync(
        InputAppearanceTarget target, long sessionId, long version, CancellationToken ct)
    {
        await resources.EnsureTargetAsync(context.EnterpriseId, target, false, ct);
        var appearance = await repository.GetAsync(context.EnterpriseId, target, ct) ?? throw NotFound();
        var session = await repository.GetSessionAsync(context.EnterpriseId, sessionId, ct) ?? throw NotFound();

        if (session.AppearanceId != appearance.Id)
            throw NotFound();

        if (appearance.MigrationBlocked)
            throw new AppearanceFailure(409, "Appearance.MigrationBlocked", "A aparência aguarda correção da migração do tema.");

        session.RequireEditable(context.UserId, version, Now);
        var release = await repository.GetReleaseAsync(session.ThemeReleaseId, ct);
        EnsureReleaseAvailable(release);

        return (appearance, session, release);
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
