namespace StoreCommerce.Domain.Models;

public sealed record ProductSearchCriteria(
    string? Term, IReadOnlyCollection<long>? CategoryIds, string? Brand,
    decimal? MinPrice, decimal? MaxPrice, bool? InStock, bool? IsActive,
    string Sort, int Page, int PageSize, bool Storefront);
