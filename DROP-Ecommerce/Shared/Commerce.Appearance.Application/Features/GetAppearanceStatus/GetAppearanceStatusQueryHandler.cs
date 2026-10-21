using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class GetAppearanceStatusQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources) : IRequestHandler<GetAppearanceStatusQuery, AppearanceStatusResponse>
{
    public async Task<AppearanceStatusResponse> Handle(GetAppearanceStatusQuery request, CancellationToken ct)
    {
        var target = request.Target;

        context.RequireAdministrator();
        await resources.EnsureTargetAsync(context.EnterpriseId, target, false, ct);
        var appearance = await repository.GetAsync(context.EnterpriseId, target, ct);

        return new AppearanceStatusResponse(TargetKey(target), appearance?.PublishedRevisionId, appearance?.Version ?? 0,
            appearance?.MigrationBlocked ?? false, appearance?.MigrationFailureCode);
    }
}
