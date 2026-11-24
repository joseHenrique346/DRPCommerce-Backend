using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Application.Interfaces.Storefront;
using DropCommerce.Application.Security;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.StaticEntity;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListStorefrontDropEventsQueryHandler(
    IDropStorefrontRepository persistence,
    ICurrentDropActor actor)
    : IRequestHandler<ListStorefrontDropEventsQuery, Result<IReadOnlyList<DropStorefrontEventSummary>>>
{
    public async Task<Result<IReadOnlyList<DropStorefrontEventSummary>>> Handle(ListStorefrontDropEventsQuery command, CancellationToken ct)
    {
        var rights = await persistence.RefreshCommercialRightsAsync(actor.EnterpriseId, ct);

        if (!rights.AllowsDrop)
            return Result<IReadOnlyList<DropStorefrontEventSummary>>.Success(Array.Empty<DropStorefrontEventSummary>());

        return Result<IReadOnlyList<DropStorefrontEventSummary>>.Success(await persistence.ListPublicEventsAsync(ct));
    }
}
