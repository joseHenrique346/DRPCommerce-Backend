using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Application.Contracts.Appearance;
using DropCommerce.Application.Appearance;
using MediatR;

namespace DropCommerce.Application.Features;

public sealed class ResolveAppearanceEventQueryHandler(
    IDropAppearanceEventRepository events,
    IAppearanceContext context) : IRequestHandler<ResolveAppearanceEventQuery, InputAppearanceTarget>
{
    public async Task<InputAppearanceTarget> Handle(ResolveAppearanceEventQuery request, CancellationToken ct)
    {
        var id = await events.GetPublicIdBySlugAsync(context.EnterpriseId, request.Slug, ct);

        if (id is not long eventId)
            throw new AppearanceFailure(404, "Appearance.TargetNotFound", "Evento indisponível.");

        return DropAppearanceTargets.Event(eventId);
    }
}
