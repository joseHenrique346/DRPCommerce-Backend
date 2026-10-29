using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AdjustStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<AdjustStockCommand, StockMovementResponse>
{
    protected override Task<StockMovementResponse> ExecuteAsync(AdjustStockCommand request, CancellationToken ct)
    {
        return inventory.AdjustAsync(request.ProductVariantId, request.Request, ct);
    }
}

public sealed class ReserveStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<ReserveStockCommand, StockReservationResponse>
{
    protected override Task<StockReservationResponse> ExecuteAsync(ReserveStockCommand request, CancellationToken ct)
    {
        return inventory.ReserveAsync(request.Request, ct);
    }
}

public sealed class ReleaseStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<ReleaseStockCommand, StockReservationResponse>
{
    protected override Task<StockReservationResponse> ExecuteAsync(ReleaseStockCommand request, CancellationToken ct)
    {
        return inventory.CompleteAsync(request.Id, false, ct);
    }
}

public sealed class CommitStockCommandHandler(InventoryService inventory) : CatalogRequestHandler<CommitStockCommand, StockReservationResponse>
{
    protected override Task<StockReservationResponse> ExecuteAsync(CommitStockCommand request, CancellationToken ct)
    {
        return inventory.CompleteAsync(request.Id, true, ct);
    }
}

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

public sealed class GetVariantStockQueryHandler(IInventoryRepository repository, ITenantProvider tenant)
    : CatalogRequestHandler<GetVariantStockQuery, ProductVariantResponse>
{
    protected override async Task<ProductVariantResponse> ExecuteAsync(GetVariantStockQuery request, CancellationToken ct)
    {
        var variant = (await repository.GetVariantsAsync(tenant.GetEnterpriseId(), [request.Id], ct)).SingleOrDefault()
            ?? throw new SecurityFailure(404, "ProductVariant.NotFound", "SKU não encontrado na loja atual.");

        return ProductVariantResponse.FromEntity(variant);
    }
}

public sealed class GetStockMovementsQueryHandler(IInventoryRepository repository, ITenantProvider tenant)
    : CatalogRequestHandler<GetStockMovementsQuery, PagedResponse<StockMovementResponse>>
{
    protected override async Task<PagedResponse<StockMovementResponse>> ExecuteAsync(GetStockMovementsQuery request, CancellationToken ct)
    {
        var (items, count) = await repository.GetMovementsAsync(tenant.GetEnterpriseId(), request.ProductVariantId, request.Page, request.PageSize, ct);

        return new(items.Select(StockMovementResponse.FromEntity).ToList(), count, request.Page, request.PageSize);
    }
}
