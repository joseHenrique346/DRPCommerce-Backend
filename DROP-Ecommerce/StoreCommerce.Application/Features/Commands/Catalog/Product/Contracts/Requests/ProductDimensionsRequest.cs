namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductDimensionsRequest(decimal Weight, decimal Height, decimal Width, decimal Length);
