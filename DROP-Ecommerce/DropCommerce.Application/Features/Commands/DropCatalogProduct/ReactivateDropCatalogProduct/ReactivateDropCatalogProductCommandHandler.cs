using System.Text.Json;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReactivateDropCatalogProductCommandHandler(
    IDropCatalogRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor)
    : IRequestHandler<ReactivateDropCatalogProductCommand, Result<bool>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<bool>> Handle(ReactivateDropCatalogProductCommand command, CancellationToken ct)
    {
        long productId = command.ProductId;
        long expectedRevision = command.ExpectedContractRevision;

        Require("drop.events.manage");
        var rights = await persistence.RefreshCommercialRightsAsync(Tenant, ct);

        if (!rights.AllowsDrop || expectedRevision != rights.ContractRevision)
            throw new SecurityFailure(409, "DropContract.ContractChanged", "Atualize a revisão e os direitos antes de reativar.");

        using var operation = writes.Begin("reactivation");
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(Tenant, ct);
        var product = await persistence.FindProductAsync(productId, ct)
            ?? throw new SecurityFailure(404, "DropCatalog.NotFound", "Produto não encontrado.");

        if (!product.IsContractBlocked)
            return Result<bool>.Success(true);

        if (await persistence.CountAvailableCatalogAsync(ct) >= rights.ProductLimit)
            throw new SecurityFailure(409, "DropContract.CreationLimit", "Não há vaga para reativar este cadastro.");

        product.Reactivate();
        persistence.RecordCommercialActivity(rights, "catalog_reactivated", product.Id, null);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<bool>.Success(true);
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
