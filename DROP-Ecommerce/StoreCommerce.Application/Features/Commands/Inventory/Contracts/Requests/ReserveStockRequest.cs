using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

public sealed record ReserveStockRequest(string IdempotencyKey, List<StockReservationItem> Items, string Reference = "");
