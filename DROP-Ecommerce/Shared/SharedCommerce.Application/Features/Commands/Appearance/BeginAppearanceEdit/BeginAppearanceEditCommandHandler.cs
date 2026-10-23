using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;
using static SharedCommerce.Application.Mappings.AppearanceResponseMapper;

namespace SharedCommerce.Application.Features.Commands;

public sealed class BeginAppearanceEditCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    TimeProvider clock,
    IMediator mediator) : IRequestHandler<BeginAppearanceEditCommand, AppearanceEditResponse>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearanceEditResponse> Handle(BeginAppearanceEditCommand request, CancellationToken ct)
    {
        var target = request.Target;
        var payload = request.Request;

        context.RequireAdministrator();
        var tenant = context.EnterpriseId;
        await using var transaction = await repository.BeginTransactionAsync(tenant, ct);
        await resources.EnsureTargetAsync(tenant, target, false, ct);
        var appearance = await GetOrCreateAsync(tenant, target, ct);

        if (appearance.MigrationBlocked)
            throw new AppearanceFailure(409, "Appearance.MigrationBlocked", "A aparência aguarda correção da migração do tema.");

        await RequireUnlockedAsync(tenant, appearance.Id, ct);
        var release = await repository.GetReleaseAsync(payload.ThemeKey, payload.ThemeVersion, ct)
            ?? throw new AppearanceFailure(404, "Appearance.ThemeNotFound", "Tema não encontrado para este módulo.");
        EnsureReleaseAvailable(release);
        var current = (await mediator.Send(new ResolvePublishedAppearanceQuery(tenant, target), ct))?.Revision;
        var settings = !payload.UseDefaults && current?.ThemeReleaseId == release.Id
            ? current.SettingsJson
            : release.DefaultsJson;
        var session = AppearanceEditSession.Begin(
            tenant, appearance.Id, release.Id, context.UserId, appearance.PublishedRevisionId, settings, Now);
        repository.Add(session);
        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);

        return EditResponse(session, release);
    }

    private async Task<AppearanceEntity> GetOrCreateAsync(long tenant, InputAppearanceTarget target, CancellationToken ct)
    {
        var appearance = await repository.GetAsync(tenant, target, ct);

        if (appearance is not null)
            return appearance;

        appearance = AppearanceEntity.Create(tenant, target.AppearanceTargetTypeId, target.DropEventId);
        repository.Add(appearance);
        await repository.SaveAsync(ct);

        return appearance;
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

    private void EnsureReleaseAvailable(ThemeRelease release)
    {
        if (release.AppearanceChannelId != repository.Channel.Id || release.RendererContract != 1 || release.MigrationDeadline <= Now)
            throw new AppearanceFailure(409, "Appearance.ReleaseUnavailable", "A versão do tema não está disponível para novas publicações.");
    }
}
