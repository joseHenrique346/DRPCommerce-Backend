using System.Text.Json;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class CreateDropCatalogProductCommandHandler(
    IDropCatalogRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor)
    : IRequestHandler<CreateDropCatalogProductCommand, Result<DropCatalogProduct>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<DropCatalogProduct>> Handle(CreateDropCatalogProductCommand command, CancellationToken ct)
    {
        CreateDropCatalogProduct request = command.Input;

        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        using var operation = writes.Begin("catalog");
        await using var tx = await persistence.BeginTransactionAsync(ct);
        var product = DropCatalogProduct.Create(tenant, request.Name, request.Description);

        foreach (var variant in request.Variants)
        {

            product.Variants.Add(DropCatalogVariant.Create(product, variant.SKU,
                JsonSerializer.Serialize(variant.Options), variant.Price, variant.Quantity));
        }

        persistence.Add(product);
        await unitOfWork.CommitAsync(ct);
        persistence.RecordCommercialActivity(rights, "catalog_created", product.Id, null);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropCatalogProduct>.Success(product);
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
