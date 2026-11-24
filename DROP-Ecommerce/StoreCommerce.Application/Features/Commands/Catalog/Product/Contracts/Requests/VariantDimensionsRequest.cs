namespace StoreCommerce.Application.Contracts.Product;

public sealed record VariantDimensionsRequest(decimal? Weight, decimal? Height, decimal? Width, decimal? Length);
