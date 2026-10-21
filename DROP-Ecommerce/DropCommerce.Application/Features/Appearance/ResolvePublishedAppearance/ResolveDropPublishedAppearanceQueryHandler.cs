using Commerce.Appearance.Application;
using Commerce.Appearance.Application.Features;
using DropCommerce.Application.Appearance;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace DropCommerce.Application.Features;

public sealed class ResolveDropPublishedAppearanceQueryHandler(
    IAppearanceRepository repository,
    IDropAppearanceEventRepository events) : IRequestHandler<ResolvePublishedAppearanceQuery, OutputResolvedAppearance?>
{
    public async Task<OutputResolvedAppearance?> Handle(ResolvePublishedAppearanceQuery request, CancellationToken ct)
    {
        var tenant = request.EnterpriseId;
        var target = request.Target;
        var revisionId = request.RevisionId;

        if (target.DropEventId is not long eventId)
        {
            var appearance = await repository.GetAsync(tenant, target, ct);

            if (appearance?.PublishedRevisionId is not long publishedId)
                return null;

            var revision = await repository.GetRevisionAsync(tenant, revisionId ?? publishedId, ct);

            if (revision is null || revision.AppearanceId != appearance.Id)
                return null;

            return new OutputResolvedAppearance(revision, TargetCode(target));
        }

        var dropEvent = await events.GetAsync(tenant, eventId, ct);

        if (dropEvent is null)
            return null;

        var own = await repository.GetAsync(tenant, target, ct);
        var fallback = await repository.GetAsync(tenant, DropAppearanceTargets.Default, ct);
        var frozen = dropEvent.EffectiveAppearanceRevisionId.HasValue || dropEvent.DropEventStatusId is >= 5 and <= 7;
        var currentId = frozen
            ? dropEvent.EffectiveAppearanceRevisionId
            : own?.PublishedRevisionId ?? fallback?.PublishedRevisionId;

        if (currentId is not long publishedRevisionId)
            return null;

        var current = await repository.GetRevisionAsync(tenant, publishedRevisionId, ct);

        if (current is null)
            return null;

        if (revisionId.HasValue && revisionId.Value != publishedRevisionId)
        {
            var candidate = await repository.GetRevisionAsync(tenant, revisionId.Value, ct);

            if (candidate is null || candidate.AppearanceId != own?.Id && candidate.AppearanceId != fallback?.Id)
                return null;

            current = candidate;
        }

        var source = current.AppearanceId == own?.Id ? TargetCode(target) : TargetCode(DropAppearanceTargets.Default);

        return new OutputResolvedAppearance(current, source);
    }
}
