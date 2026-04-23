namespace StoreCommerce.Domain.Entity.Category
{
    internal class Category : BaseEntity
    {
        public long EnterpriseId { get; private set; }
        public long? ParentCategoryId { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string Dscription { get; private set; }
        public string ImageUrl { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }

        public Category() { }

        public Category(long enterpriseId, long? parentCategoryId, string name, string slug, string dscription, string imageUrl, int displayOrder, bool isActive)
        {
            EnterpriseId = enterpriseId;
            ParentCategoryId = parentCategoryId;
            Name = name;
            Slug = slug;
            Dscription = dscription;
            ImageUrl = imageUrl;
            DisplayOrder = displayOrder;
            IsActive = isActive;
        }
    }
}