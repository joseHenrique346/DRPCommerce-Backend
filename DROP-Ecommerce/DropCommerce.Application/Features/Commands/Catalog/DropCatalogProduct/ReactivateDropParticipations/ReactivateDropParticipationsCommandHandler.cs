using DropCommerce.Application.Contracts;
using DropCommerce.Application.Interfaces.Catalog;
using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Domain.Interfaces.Persistence;
using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReactivateDropParticipationsCommandHandler(
    IDropCatalogRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor,
    DropReactivationSelector selectionReader)
    : IRequestHandler<ReactivateDropParticipationsCommand, Result<DropReactivationResponse>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<DropReactivationResponse>> Handle(ReactivateDropParticipationsCommand command, CancellationToken ct)
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

        foreach (var product in selected)
        {
            product.CatalogProduct!.Reactivate();
            product.ReactivateForContract();
            persistence.RecordCommercialActivity(rights, "participation_reactivated", product.CatalogProductId, product.Id);
        }

        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropReactivationResponse>.Success(new DropReactivationResponse(rights.ContractRevision, available, selected.Select(Product => Product.Id).ToList(), true));
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
