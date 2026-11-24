namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductSearchRequest(string? Term = null, long? CategoryId = null, bool IncludeSubcategories = false,
    string? Brand = null, decimal? MinPrice = null, decimal? MaxPrice = null, bool? InStock = null, bool? IsActive = null,
    string Sort = "name", int Page = 1, int PageSize = 20);
