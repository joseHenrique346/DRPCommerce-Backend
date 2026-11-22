using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

public sealed record StockReservationLineResponse(long ProductVariantId, int Quantity, bool AllowsBackorder);
