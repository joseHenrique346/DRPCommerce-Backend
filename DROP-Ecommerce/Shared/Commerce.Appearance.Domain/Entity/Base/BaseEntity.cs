namespace Commerce.Appearance.Domain.Entity.Base;

public abstract class BaseEntity
{
    public long Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void SetChangedDate()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
