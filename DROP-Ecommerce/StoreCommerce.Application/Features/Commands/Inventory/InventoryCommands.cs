using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record AdjustStockCommand(long ProductVariantId, AdjustStockRequest Request) : IRequest<Result<StockMovementResponse>>;
public sealed record ReserveStockCommand(ReserveStockRequest Request) : IRequest<Result<StockReservationResponse>>;
public sealed record ReleaseStockCommand(long Id) : IRequest<Result<StockReservationResponse>>;
public sealed record CommitStockCommand(long Id) : IRequest<Result<StockReservationResponse>>;
public sealed record GetStockReservationQuery(long Id) : IRequest<Result<StockReservationResponse>>;
public sealed record GetVariantStockQuery(long Id) : IRequest<Result<ProductVariantResponse>>;
public sealed record GetStockMovementsQuery(long? ProductVariantId = null, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResponse<StockMovementResponse>>>;
