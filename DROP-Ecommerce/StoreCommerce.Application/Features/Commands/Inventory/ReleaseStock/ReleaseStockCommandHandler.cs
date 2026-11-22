using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReleaseStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<ReleaseStockCommand, StockReservationResponse>
{
    protected override Task<StockReservationResponse> ExecuteAsync(ReleaseStockCommand request, CancellationToken ct)
    {
        return inventory.CompleteAsync(request.Id, false, ct);
    }
}
