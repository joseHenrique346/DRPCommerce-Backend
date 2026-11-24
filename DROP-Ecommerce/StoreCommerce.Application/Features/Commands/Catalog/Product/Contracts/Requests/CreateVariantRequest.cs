namespace StoreCommerce.Application.Contracts.Product;

public sealed record CreateVariantRequest(VariantDetailsRequest Details, VariantPricingRequest? Pricing = null, VariantDimensionsRequest? Dimensions = null);
