using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;
using AppearanceEntity = Commerce.Appearance.Domain.Appearance;

namespace Commerce.Appearance.Application.Features;

public sealed class SaveAppearanceCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    AppearanceSettingsValidator settingsValidator,
    TimeProvider clock) : IRequestHandler<SaveAppearanceCommand, AppearanceEditResponse>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearanceEditResponse> Handle(SaveAppearanceCommand request, CancellationToken ct)
    {
        var target = request.Target;
        var sessionId = request.SessionId;
        var version = request.Version;
        var payload = request.Request;

        context.RequireAdministrator();
        await using var transaction = await repository.BeginTransactionAsync(context.EnterpriseId, ct);
        var (_, session, release) = await RequireSessionAsync(target, sessionId, version, ct);

        if (payload.Settings.ValueKind != System.Text.Json.JsonValueKind.Object)
            throw new AppearanceFailure(400, "Appearance.InvalidSettings", "Envie um objeto de configurações completo.");

        var settings = settingsValidator.Validate(release.SchemaJson, payload.Settings.GetRawText());
        session.Save(settings, Now);
        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);

        return EditResponse(session, release);
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
