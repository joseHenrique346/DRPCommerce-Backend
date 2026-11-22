using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReserveStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<ReserveStockCommand, StockReservationResponse>
{
    protected override Task<StockReservationResponse> ExecuteAsync(ReserveStockCommand request, CancellationToken ct)
    {
        return inventory.ReserveAsync(request.Request, ct);
    }
}
