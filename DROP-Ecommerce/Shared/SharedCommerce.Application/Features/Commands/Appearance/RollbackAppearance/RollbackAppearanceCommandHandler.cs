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

public sealed class RollbackAppearanceCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    AppearanceSettingsValidator settingsValidator,
    AppearanceResourceValidator resourceValidator,
    TimeProvider clock,
    IMediator mediator) : IRequestHandler<RollbackAppearanceCommand, AppearanceManifest>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearanceManifest> Handle(RollbackAppearanceCommand request, CancellationToken ct)
    {
        var target = request.Target;
        var revisionId = request.RevisionId;
        var payload = request.Request;

        context.RequireAdministrator();
        await using var transaction = await repository.BeginTransactionAsync(context.EnterpriseId, ct);
        await resources.EnsureTargetAsync(context.EnterpriseId, target, false, ct);
        var appearance = await repository.GetAsync(context.EnterpriseId, target, ct) ?? throw NotFound();
        var previous = await repository.GetRevisionAsync(context.EnterpriseId, revisionId, ct) ?? throw NotFound();

        if (previous.AppearanceId != appearance.Id)
            throw NotFound();

        if (appearance.PublishedRevisionId != payload.ExpectedRevisionId)
            throw new AppearanceFailure(412, "Appearance.VersionConflict", "A publicação mudou. Recarregue antes de restaurar.");

        await RequireUnlockedAsync(context.EnterpriseId, appearance.Id, ct);
        var release = await repository.GetReleaseAsync(previous.ThemeReleaseId, ct);
        var revision = await CreateRevisionAsync(appearance, target, release, previous.SettingsJson,
            context.UserId, previous.Id, AppearanceRevisionOrigin.Rollback, ct);
        await mediator.Publish(new AppearancePublishedNotification(appearance.EnterpriseId, target, revision.Id), ct);
        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);

        return Manifest(repository.Channel, revision, release, revision.SettingsJson, TargetCode(target), revision.PublishedAt);
    }

    private async Task RequireUnlockedAsync(long tenant, long appearanceId, CancellationToken ct)
    {
        var session = await repository.GetOpenSessionAsync(tenant, appearanceId, ct);

        if (session is null)
            return;

        if (session.ExpiresAt > Now)
            throw new AppearanceFailure(409, "Appearance.Locked", $"A aparência está em edição pelo usuário {session.EditorId} até {session.ExpiresAt:O}.");

        session.Close(AppearanceEditSessionStatus.Expired);
        await repository.SaveAsync(ct);
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
