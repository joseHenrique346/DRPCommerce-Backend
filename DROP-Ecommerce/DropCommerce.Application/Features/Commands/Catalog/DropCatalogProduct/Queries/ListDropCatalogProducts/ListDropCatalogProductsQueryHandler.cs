using DropCommerce.Application.Interfaces.Catalog;
using DropCommerce.Application.Interfaces.Identity;
using System.Text.Json;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListDropCatalogProductsQueryHandler(
    IDropCatalogRepository persistence,
    ICurrentDropActor actor)
    : IRequestHandler<ListDropCatalogProductsQuery, Result<IReadOnlyList<DropCatalogProduct>>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<IReadOnlyList<DropCatalogProduct>>> Handle(ListDropCatalogProductsQuery command, CancellationToken ct)
    {
        long? afterId = command.AfterId;

        Require("drop.events.view");
        await persistence.RefreshCommercialRightsAsync(Tenant, ct);

        return Result<IReadOnlyList<DropCatalogProduct>>.Success(await persistence.ListProductsAsync(afterId, ct));
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
