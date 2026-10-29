namespace StoreCommerce.Application.Contracts.Category;

public sealed record CategoryResponse(
    long Id, long? ParentCategoryId, string Name, string Slug,
    string Description, string ImageUrl, int DisplayOrder, bool IsActive)
{
    public static CategoryResponse FromEntity(Domain.Entity.Category category)
    {
        return new(category.Id, category.ParentCategoryId, category.Name, category.Slug,
            category.Description, category.ImageUrl, category.DisplayOrder, category.IsActive);
    }
}
