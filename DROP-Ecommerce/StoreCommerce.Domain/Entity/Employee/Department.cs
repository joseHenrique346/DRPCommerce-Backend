namespace StoreCommerce.Domain.Entity;

public class Department : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Department() { }

    public Department(string name, string description)
    {
        Name = name;
        Description = description;
    }
}