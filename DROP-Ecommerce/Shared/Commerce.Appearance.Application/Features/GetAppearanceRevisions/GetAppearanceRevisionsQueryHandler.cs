using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class GetAppearanceRevisionsQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources) : IRequestHandler<GetAppearanceRevisionsQuery, IReadOnlyList<AppearanceRevisionResponse>>
{
    public async Task<IReadOnlyList<AppearanceRevisionResponse>> Handle(GetAppearanceRevisionsQuery request, CancellationToken ct)
    {
        var target = request.Target;
        var beforeNumber = request.BeforeNumber;

        context.RequireAdministrator();
        await resources.EnsureTargetAsync(context.EnterpriseId, target, false, ct);
        var appearance = await repository.GetAsync(context.EnterpriseId, target, ct);

        if (appearance is null)
            return [];

        var revisions = await repository.GetRevisionsAsync(context.EnterpriseId, appearance.Id, ct, beforeNumber);
        var releases = (await repository.GetReleasesAsync(ct)).ToDictionary(Release => Release.Id);

        return revisions.Select(Revision => new AppearanceRevisionResponse(
            Revision.Id, Revision.Number, Identity(releases[Revision.ThemeReleaseId]), Revision.Checksum,
            Revision.PublishedBy, Revision.PublishedAt, Revision.RestoredFromRevisionId,
            BaseStaticEntity.FromId<AppearanceRevisionOrigin>(Revision.AppearanceRevisionOriginId).Code)).ToList();
    }
}
