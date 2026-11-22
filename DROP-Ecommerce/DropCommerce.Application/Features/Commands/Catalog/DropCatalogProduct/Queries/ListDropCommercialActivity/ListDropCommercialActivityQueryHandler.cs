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

public sealed class ListDropCommercialActivityQueryHandler(
    IDropCatalogRepository persistence,
    ICurrentDropActor actor)
    : IRequestHandler<ListDropCommercialActivityQuery, Result<IReadOnlyList<DropCommercialActivity>>>
{
    public async Task<Result<IReadOnlyList<DropCommercialActivity>>> Handle(ListDropCommercialActivityQuery command, CancellationToken ct)
    {
        long? beforeId = command.BeforeId;

        Require("store.history.view");

        return Result<IReadOnlyList<DropCommercialActivity>>.Success(await persistence.ListActivityAsync(beforeId, ct));
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
