namespace DropCommerce.Application.Contracts;

public sealed record DropPurchaseItemResponse(
    string ItemName,
    string SKU,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? OptionsSnapshotJson);
