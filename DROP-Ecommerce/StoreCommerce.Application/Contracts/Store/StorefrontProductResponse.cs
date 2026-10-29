namespace StoreCommerce.Application.Contracts.Store;

public sealed record StorefrontProductResponse(
    long Id, string Name, string Slug, string Description,
    decimal Price, string Brand, string ImageUrls, long CategoryId);
