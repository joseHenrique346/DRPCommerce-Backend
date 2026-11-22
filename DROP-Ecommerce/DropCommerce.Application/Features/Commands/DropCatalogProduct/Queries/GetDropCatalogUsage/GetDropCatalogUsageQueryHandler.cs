using System.Text.Json;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class GetDropCatalogUsageQueryHandler(
    IDropCatalogRepository persistence,
    ICurrentDropActor actor)
    : IRequestHandler<GetDropCatalogUsageQuery, Result<DropCatalogUsageResponse>>
{
    private long Tenant => actor.EnterpriseId;

    public async Task<Result<DropCatalogUsageResponse>> Handle(GetDropCatalogUsageQuery command, CancellationToken ct)
    {
        Require("drop.events.view");
        var rights = await persistence.RefreshCommercialRightsAsync(Tenant, ct);
        var catalog = await persistence.ListCatalogAsync(ct);
        var products = await persistence.ListParticipationsAsync(ct);
        var published = products.Where(Product => DropEventPublication.IsPublished(Product.DropEvent) && Product.IsActive).ToList();

        return Result<DropCatalogUsageResponse>.Success(new DropCatalogUsageResponse(
            rights.ContractRevision, rights.AllowsDrop, rights.ProductLimit,
            catalog.Count, rights.AllowsDrop && catalog.Count < rights.ProductLimit,
            published.Where(Product => !Product.IsContractBlocked)
                .Select(Product => (Product.DropEventId, Product.CatalogProductId)).Distinct().Count(),
            products.Count(Product => Product.IsContractBlocked),
            catalog.Count(Product => Product.StoreProductId.HasValue && !Product.SourceExportId.HasValue),
            catalog.Count(Product => Product.IsReturning)));
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
