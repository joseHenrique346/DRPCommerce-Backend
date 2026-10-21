using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class ResolvePublishedAppearanceQueryHandler(IAppearanceRepository repository)
    : IRequestHandler<ResolvePublishedAppearanceQuery, OutputResolvedAppearance?>
{
    public async Task<OutputResolvedAppearance?> Handle(ResolvePublishedAppearanceQuery request, CancellationToken ct)
    {
        var appearance = await repository.GetAsync(request.EnterpriseId, request.Target, ct);

        if (appearance?.PublishedRevisionId is not long publishedId)
            return null;

        var revision = await repository.GetRevisionAsync(request.EnterpriseId, request.RevisionId ?? publishedId, ct);

        if (revision is null || revision.AppearanceId != appearance.Id)
            return null;

        return new OutputResolvedAppearance(revision, TargetCode(request.Target));
    }
}
