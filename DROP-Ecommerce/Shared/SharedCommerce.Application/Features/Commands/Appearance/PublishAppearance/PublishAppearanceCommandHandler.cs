using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Features.Notifications;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;
using static SharedCommerce.Application.Mappings.AppearanceResponseMapper;

namespace SharedCommerce.Application.Features.Commands;

public sealed class PublishAppearanceCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    AppearanceSettingsValidator settingsValidator,
    AppearanceResourceValidator resourceValidator,
    TimeProvider clock,
    IMediator mediator) : IRequestHandler<PublishAppearanceCommand, AppearanceManifest>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearanceManifest> Handle(PublishAppearanceCommand request, CancellationToken ct)
    {
        var target = request.Target;
        var sessionId = request.SessionId;
        var version = request.Version;

        context.RequireAdministrator();
        await using var transaction = await repository.BeginTransactionAsync(context.EnterpriseId, ct);
        var (appearance, session, release) = await RequireSessionAsync(target, sessionId, version, ct);

        if (appearance.PublishedRevisionId != session.BaseRevisionId)
            throw new AppearanceFailure(409, "Appearance.BaseChanged", "A versão publicada mudou desde o início da edição.");

        var revision = await CreateRevisionAsync(appearance, target, release, session.SettingsJson,
            context.UserId, session.RestoredFromRevisionId, AppearanceRevisionOrigin.Publish, ct);
        await mediator.Publish(new AppearancePublishedNotification(appearance.EnterpriseId, target, revision.Id), ct);
        session.Close(AppearanceEditSessionStatus.Published);
        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);

        return Manifest(repository.Channel, revision, release, revision.SettingsJson, TargetCode(target), revision.PublishedAt);
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
