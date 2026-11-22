namespace StoreCommerce.Application.Contracts.Product;

public sealed record VariantPricingRequest(decimal? Price, decimal? CostPrice);
