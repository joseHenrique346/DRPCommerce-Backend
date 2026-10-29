namespace StoreCommerce.Application.Contracts.Category;

public sealed record CategoryTreeResponse(
    long Id, long? ParentCategoryId, string Name, string Slug,
    string Description, string ImageUrl, int DisplayOrder, bool IsActive,
    List<CategoryTreeResponse> Children);
