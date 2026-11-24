namespace DropCommerce.Domain.Entity;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        var now = DateTime.UtcNow;
        CreatedAt = new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
    }

    protected void PreserveCreationDate(DateTime original)
    {
        if (original.Kind != DateTimeKind.Utc || original > DateTime.UtcNow)
            throw new ArgumentException("Data de origem inválida.");

        CreatedAt = original;
    }

    #region Properties
    public long Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    #endregion

    #region Functions
    public void SetChangedDate()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}
