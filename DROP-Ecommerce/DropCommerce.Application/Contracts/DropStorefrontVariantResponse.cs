namespace DropCommerce.Application.Contracts;

public sealed record DropStorefrontVariantResponse(
    long Id,
    string SKU,
    string OptionsJson,
    decimal Price,
    int Available);
