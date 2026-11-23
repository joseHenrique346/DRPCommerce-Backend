using DropCommerce.Application.Contracts;
using DropCommerce.Application.Interfaces.Catalog;
using DropCommerce.Application.Interfaces.Identity;
using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed class PreviewDropReactivationQueryHandler(
    IDropCatalogRepository persistence,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor,
    DropReactivationSelector selectionReader)
    : IRequestHandler<PreviewDropReactivationQuery, Result<DropReactivationResponse>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<DropReactivationResponse>> Handle(PreviewDropReactivationQuery command, CancellationToken ct)
    {
        var request = command.Input;

        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop || request.ExpectedContractRevision != rights.ContractRevision)
            throw new SecurityFailure(409, "DropContract.RevisionChanged", "Atualize os direitos e o modo de reativação.");

        using var operation = writes.Begin("reactivation");
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var selection = await selectionReader.SelectAsync(request, rights, ct);
        var available = selection.Available;
        var selected = selection.Products;

        return Result<DropReactivationResponse>.Success(new DropReactivationResponse(rights.ContractRevision, available, selected.Select(Product => Product.Id).ToList(), false));
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
