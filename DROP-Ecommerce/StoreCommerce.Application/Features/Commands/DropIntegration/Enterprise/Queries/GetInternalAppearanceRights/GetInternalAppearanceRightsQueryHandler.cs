using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using StoreCommerce.Application.Interfaces.Appearance;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetInternalAppearanceRightsQueryHandler(
    IEnterpriseAppearanceReader reader)
    : IRequestHandler<GetInternalAppearanceRightsQuery, AppearanceCapabilities>
{
    public async Task<AppearanceCapabilities> Handle(GetInternalAppearanceRightsQuery request, CancellationToken ct)
    {
        return await reader.ReadAsync(request.EnterpriseId, request.Channel, ct);
    }
}
