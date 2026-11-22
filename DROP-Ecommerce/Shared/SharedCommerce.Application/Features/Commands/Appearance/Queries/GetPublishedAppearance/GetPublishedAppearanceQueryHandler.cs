using SharedCommerce.Application.Interfaces.Appearance;
using SharedCommerce.Application.Interfaces.Media;
using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Exceptions;
using static SharedCommerce.Application.Mappings.AppearanceResponseMapper;

namespace SharedCommerce.Application.Features.Commands;

public sealed class GetPublishedAppearanceQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    AppearanceCommercialPolicy policy,
    IMediaAssetReader media,
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
        var rights = await policy.GetAsync(tenant, ct);

        if (!rights.AllowsModule || rights.ThemeLimit <= 0)
            throw NotFound();

        var published = await mediator.Send(new ResolvePublishedAppearanceQuery(tenant, target, revisionId), ct);
        var revision = published?.Revision;
        var release = revision is null ? null : await repository.GetReleaseAsync(revision.ThemeReleaseId, ct);
        var source = published?.Source ?? "platform-default";

        if (release is not null && !AppearanceCommercialSettings.Allows(release, rights))
        {
            if (revisionId.HasValue)
                throw NotFound();

            revision = await repository.GetLatestEligibleRevisionAsync(tenant, revision!.AppearanceId,
                revision.Number, rights.ThemeLimit, ct);
            release = revision is null ? null : await repository.GetReleaseAsync(revision.ThemeReleaseId, ct);
            source = "contract-fallback";
        }

        if (release is null)
        {
            if (revisionId.HasValue)
                throw NotFound();

            release = (await repository.GetReleasesAsync(ct)).Where(Release => Release.CatalogPosition == 1 &&
                Release.RendererContract == 1 && (Release.MigrationDeadline is null || Release.MigrationDeadline > DateTime.UtcNow))
                .OrderByDescending(Release => Version.Parse(Release.Version)).FirstOrDefault() ?? throw NotFound();
        }

        var settings = AppearanceCommercialSettings.Project(release, revision?.SettingsJson ?? release.DefaultsJson, rights);
        settings = await resources.FilterPublishedSettingsAsync(tenant, release.SchemaJson, settings, ct);
        var manifest = Manifest(repository.Channel, revision, release, settings, source, revision?.PublishedAt ?? release.CreatedAt);

        manifest = manifest with { Checksum = ConfigurationChecksum(release, settings) };

        return await SharedCommerce.Application.Mappings.AppearanceAssetBindings.ResolveAsync(media, manifest, tenant, ct);
    }

    private static AppearanceFailure NotFound()
    {
        return new AppearanceFailure(404, "Appearance.NotFound", "Aparência ou revisão indisponível.");
    }
}
