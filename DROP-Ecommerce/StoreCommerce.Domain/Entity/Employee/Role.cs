namespace StoreCommerce.Domain.Entity;

public class Role : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Role() { }

    public Role(string name, string description)
    {
        Name = name;
        Description = description;
    }
}