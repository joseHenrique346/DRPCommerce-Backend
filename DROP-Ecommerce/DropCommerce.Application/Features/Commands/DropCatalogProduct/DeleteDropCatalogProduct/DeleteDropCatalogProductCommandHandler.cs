using System.Text.Json;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class DeleteDropCatalogProductCommandHandler(
    IDropCatalogRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor)
    : IRequestHandler<DeleteDropCatalogProductCommand, Result<bool>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<bool>> Handle(DeleteDropCatalogProductCommand command, CancellationToken ct)
    {
        long productId = command.ProductId;

        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);
        using var operation = writes.Begin("catalog");
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var product = await persistence.FindProductWithVariantsAsync(productId, ct)
            ?? throw new SecurityFailure(404, "DropCatalog.NotFound", "Produto não encontrado.");
        var participations = await persistence.ListProductParticipationsAsync(productId, ct);
        var ids = participations.Select(Product => Product.Id).ToList();

        if (product.StoreProductId.HasValue && !product.IsReturned || product.Variants.Any(Variant => Variant.QuantityReserved != 0 ||
            !product.IsContractBlocked && Variant.QuantityOnHand != 0) ||
            await persistence.HasReservationsAsync(ids, ct) ||
            await persistence.HasPendingOrdersAsync(ids, ct))
            throw new SecurityFailure(409, "DropCatalog.StockInUse", "Resolva as reservas e o controle de estoque antes de excluir.");

        product.Delete(DateTime.UtcNow);

        foreach (var participation in participations)
            participation.SoftDelete();

        persistence.RecordCommercialActivity(rights, "catalog_deleted", product.Id, null);
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
