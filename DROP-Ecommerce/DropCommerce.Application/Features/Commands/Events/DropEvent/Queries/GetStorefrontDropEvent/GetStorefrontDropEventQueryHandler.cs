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

public sealed class GetStorefrontDropEventQueryHandler(
    IDropStorefrontRepository persistence,
    ICurrentDropActor actor)
    : IRequestHandler<GetStorefrontDropEventQuery, Result<DropStorefrontEventResponse?>>
{
    public async Task<Result<DropStorefrontEventResponse?>> Handle(GetStorefrontDropEventQuery command, CancellationToken ct)
    {
        long id = command.Id;

        var rights = await persistence.RefreshCommercialRightsAsync(actor.EnterpriseId, ct);

        if (!rights.AllowsDrop)
            return Result<DropStorefrontEventResponse?>.Success(null);

        var value = await persistence.FindPublicEventAsync(id, ct);

        return Result<DropStorefrontEventResponse?>.Success(value is null || !value.Products.Any() ? null : value);
    }
}
