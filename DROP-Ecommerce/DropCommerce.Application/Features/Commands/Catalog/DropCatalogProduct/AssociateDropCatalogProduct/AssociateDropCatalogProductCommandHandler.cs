using DropCommerce.Application.Interfaces.Catalog;
using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Domain.Interfaces.Persistence;
using System.Text.Json;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class AssociateDropCatalogProductCommandHandler(
    IDropCatalogRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor)
    : IRequestHandler<AssociateDropCatalogProductCommand, Result<DropProduct>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<DropProduct>> Handle(AssociateDropCatalogProductCommand command, CancellationToken ct)
    {
        long productId = command.ProductId;
        AssociateDropCatalogProduct request = command.Input;

        Require("drop.events.manage");
        var tenant = Tenant;
        await persistence.RefreshCommercialRightsAsync(tenant, ct);
        using var operation = writes.Begin("catalog");
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var product = await persistence.FindProductWithVariantsAsync(productId, ct)
            ?? throw new SecurityFailure(404, "DropCatalog.NotFound", "Produto DROP não encontrado.");

        if (product.IsContractBlocked || product.IsReturning || product.IsReturned || product.StoreProductId.HasValue && !product.SourceExportId.HasValue)
            throw new SecurityFailure(409, "DropCatalog.Unavailable", "O produto está bloqueado ou aguarda a transferência coordenada de estoque.");

        var drop = await persistence.FindEventAsync(request.DropEventId, ct)
            ?? throw new SecurityFailure(404, "Drop.NotFound", "Evento não encontrado.");
        var existing = await persistence.FindParticipationAsync(drop.Id, product.Id, ct);

        if (existing is not null)
        {
            if (existing.MaxPerCustomer != request.MaxPerCustomer)
                throw new SecurityFailure(409, "DropCatalog.AssociationExists", "A participação já existe com outro limite de compra.");

            return Result<DropProduct>.Success(existing);
        }

        var available = product.Variants.Sum(Variant => (long)Variant.QuantityOnHand - Variant.QuantityReserved);

        if (available <= 0 || available > int.MaxValue)
            throw new SecurityFailure(400, "DropCatalog.InvalidAllocation", "Informe estoque positivo e limite de compra válido.");

        var participation = DropProduct.Create(drop.Id, null, product.Variants.First().SKU, (int)available,
            0, request.MaxPerCustomer, product.Variants.Min(Variant => Variant.Price), true);
        participation.BindCatalog(product);
        persistence.Add(participation);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropProduct>.Success(participation);
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
