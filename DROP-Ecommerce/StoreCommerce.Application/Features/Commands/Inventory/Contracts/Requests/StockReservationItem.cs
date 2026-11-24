using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

public sealed record StockReservationItem(long ProductVariantId, int Quantity);
