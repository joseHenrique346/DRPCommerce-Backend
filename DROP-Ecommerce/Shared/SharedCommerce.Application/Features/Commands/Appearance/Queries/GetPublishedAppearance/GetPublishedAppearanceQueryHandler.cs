using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.Exceptions;
using static SharedCommerce.Application.Mappings.AppearanceResponseMapper;

namespace SharedCommerce.Application.Features.Commands;

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

        var settings = await resources.FilterPublishedSettingsAsync(tenant, release.SchemaJson, revision.SettingsJson, ct);
        var manifest = Manifest(repository.Channel, revision, release, settings, published.Source, revision.PublishedAt);

        if (settings != revision.SettingsJson)
            manifest = manifest with { Checksum = ConfigurationChecksum(release, settings) };

        return manifest;
    }

    private static AppearanceFailure NotFound()
    {
        return new AppearanceFailure(404, "Appearance.NotFound", "Aparência ou revisão indisponível.");
    }
}
