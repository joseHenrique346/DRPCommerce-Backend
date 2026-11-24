namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductDetailsRequest(string Name, string Slug, string Description, string SKU, string BarCode, string Brand, string ImageUrls, bool IsDigital);
