using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Inventory;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetStockReservationQueryHandler(IInventoryRepository repository, ITenantProvider tenant)
    : CatalogRequestHandler<GetStockReservationQuery, StockReservationResponse>
{
    protected override async Task<StockReservationResponse> ExecuteAsync(GetStockReservationQuery request, CancellationToken ct)
    {
        var reservation = await repository.GetReservationAsync(tenant.GetEnterpriseId(), request.Id, ct, tracking: false)
            ?? throw new SecurityFailure(404, "Inventory.ReservationNotFound", "Reserva não encontrada na loja atual.");

        return StockReservationResponse.FromEntity(reservation);
    }
}
