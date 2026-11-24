namespace DropCommerce.Application.Contracts;

public sealed record CreateDropCatalogVariant(string SKU, Dictionary<string, string> Options, decimal Price, int Quantity);
