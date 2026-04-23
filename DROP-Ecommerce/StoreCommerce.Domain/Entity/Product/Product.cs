namespace StoreCommerce.Domain.Entity.Product
{
    public class Product : BaseEntity
    {
        public long EnterpriseId { get; private set; }
        public long CategoryId { get; private set; }
        public long? SupplierId { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string Description { get; private set; }
        public string SKU { get; private set; }
        public string BarCode { get; private set; }
        public decimal Price { get; private set; }
        public decimal CostPrice { get; private set; }
        public decimal Weight { get; private set; }
        public decimal Height { get; private set; }
        public decimal Width { get; private set; }
        public decimal Length { get; private set; }
        public string Brand { get; private set; }
        public string ImageUrls { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDigital { get; private set; }

        public Product() { }

        public Product(long enterpriseId, long categoryId, long? supplierId, string name, string slug, string sku, string barCode, decimal price, decimal costPrice, decimal weight, decimal height, decimal width, decimal length, string brand, string imageUrls, bool isActive, bool isDigita)
        {
            EnterpriseId = enterpriseId;
            CategoryId = categoryId;
            SupplierId = supplierId;
            Name = name;
            Slug = slug;
            SKU = sku;
            BarCode = barCode;
            Price = price;
            CostPrice = costPrice;
            Weight = weight;
            Height = height;
            Width = width;
            Length = length;
            Brand = brand;
            ImageUrls = imageUrls;
            IsActive = isActive;
            IsDigital = isDigita;

        }
    }
}