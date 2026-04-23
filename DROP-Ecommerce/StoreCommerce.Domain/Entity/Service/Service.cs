namespace StoreCommerce.Domain.Entity.Service
{
    public class Service : BaseEntity
    {
        public long EnterpriseId { get; private set; }
        public long CategoryId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int DurationMinutes { get; private set; }
        public bool IsActive { get; private set; }

        public Service() { }

        public Service(long enterpriseId, long categoryId, string name, string description, decimal price, int durationMinutes, bool isActive)
        {
            EnterpriseId = enterpriseId;
            CategoryId = categoryId;
            Name = name;
            Description = description;
            Price = price;
            DurationMinutes = durationMinutes;
            IsActive = isActive;
        }

    }
}