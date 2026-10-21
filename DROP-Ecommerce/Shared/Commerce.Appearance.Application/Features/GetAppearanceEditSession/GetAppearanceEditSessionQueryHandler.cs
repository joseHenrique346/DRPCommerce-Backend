using Commerce.Appearance.Contracts;
using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using MediatR;
using static Commerce.Appearance.Application.AppearanceResponseMapper;

namespace Commerce.Appearance.Application.Features;

public sealed class GetAppearanceEditSessionQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    IAppearanceResources resources,
    TimeProvider clock) : IRequestHandler<GetAppearanceEditSessionQuery, AppearanceEditResponse?>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<AppearanceEditResponse?> Handle(GetAppearanceEditSessionQuery request, CancellationToken ct)
    {
        var target = request.Target;

        context.RequireAdministrator();
        var tenant = context.EnterpriseId;
        await resources.EnsureTargetAsync(tenant, target, false, ct);
        var appearance = await repository.GetAsync(tenant, target, ct);

        if (appearance is null)
            return null;

        var session = await repository.GetOpenSessionAsync(tenant, appearance.Id, ct);

        if (session is null || session.ExpiresAt <= Now)
            return null;

        return EditResponse(session, await repository.GetReleaseAsync(session.ThemeReleaseId, ct));
    }
}
