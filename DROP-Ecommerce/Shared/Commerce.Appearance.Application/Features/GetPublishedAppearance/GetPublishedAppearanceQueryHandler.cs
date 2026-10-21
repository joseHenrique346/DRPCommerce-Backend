using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class GetPublishedAppearanceQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    IMediator mediator) : IRequestHandler<GetPublishedAppearanceQuery, AppearanceManifest>
{
    public async Task<AppearanceManifest> Handle(GetPublishedAppearanceQuery request, CancellationToken ct)
    {
        var target = request.Target;
        var revisionId = request.RevisionId;

        var tenant = context.EnterpriseId;

        if (tenant < 1)
            throw NotFound();

        await resources.EnsureTargetAsync(tenant, target, true, ct);
        var published = await mediator.Send(new ResolvePublishedAppearanceQuery(tenant, target, revisionId), ct) ?? throw NotFound();
        var revision = published.Revision;
        var release = await repository.GetReleaseAsync(revision.ThemeReleaseId, ct);

        return Manifest(repository.Channel, revision, release, revision.SettingsJson, published.Source, revision.PublishedAt);
    }

    private static AppearanceFailure NotFound()
    {
        return new AppearanceFailure(404, "Appearance.NotFound", "Aparência ou revisão indisponível.");
    }
}
