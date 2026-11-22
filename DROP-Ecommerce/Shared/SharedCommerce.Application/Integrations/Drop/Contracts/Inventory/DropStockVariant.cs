namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropStockVariant(long SourceVariantId, string SKU, string OptionsJson, decimal Price, int Quantity);
