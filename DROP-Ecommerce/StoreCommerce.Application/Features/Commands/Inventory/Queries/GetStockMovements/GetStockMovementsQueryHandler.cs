using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Inventory;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetStockMovementsQueryHandler(IInventoryRepository repository, ITenantProvider tenant)
    : CatalogRequestHandler<GetStockMovementsQuery, PagedResponse<StockMovementResponse>>
{
    protected override async Task<PagedResponse<StockMovementResponse>> ExecuteAsync(GetStockMovementsQuery request, CancellationToken ct)
    {
        var (items, count) = await repository.GetMovementsAsync(tenant.GetEnterpriseId(), request.ProductVariantId, request.Page, request.PageSize, ct);

        return new(items.Select(StockMovementResponse.FromEntity).ToList(), count, request.Page, request.PageSize);
    }
}
