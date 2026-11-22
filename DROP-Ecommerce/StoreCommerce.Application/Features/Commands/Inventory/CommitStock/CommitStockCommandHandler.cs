using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CommitStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<CommitStockCommand, StockReservationResponse>
{
    protected override Task<StockReservationResponse> ExecuteAsync(CommitStockCommand request, CancellationToken ct)
    {
        return inventory.CompleteAsync(request.Id, true, ct);
    }
}
