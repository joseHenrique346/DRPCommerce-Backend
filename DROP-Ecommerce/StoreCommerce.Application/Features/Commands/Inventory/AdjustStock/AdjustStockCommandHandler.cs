using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AdjustStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<AdjustStockCommand, StockMovementResponse>
{
    protected override Task<StockMovementResponse> ExecuteAsync(AdjustStockCommand request, CancellationToken ct)
    {
        return inventory.AdjustAsync(request.ProductVariantId, request.Request, ct);
    }
}
