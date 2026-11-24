namespace StoreCommerce.Application.Contracts.Product;

public sealed record VariantDetailsRequest(string Name, string SKU, string BarCode, Dictionary<string, string> Attributes, bool IsActive = true);
