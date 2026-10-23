using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using static SharedCommerce.Application.Mappings.AppearanceResponseMapper;

namespace SharedCommerce.Application.Features.Commands;

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
