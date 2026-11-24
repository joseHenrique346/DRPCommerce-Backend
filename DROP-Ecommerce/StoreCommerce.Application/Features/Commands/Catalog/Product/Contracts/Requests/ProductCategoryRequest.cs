namespace StoreCommerce.Application.Contracts.Product;

public sealed record ProductCategoryRequest(long CategoryId, long? SupplierId);
