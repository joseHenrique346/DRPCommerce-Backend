using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Inventory;

public sealed record AdjustStockRequest(int QuantityChange, string Reason, string IdempotencyKey, long? ExpectedVersion = null);
