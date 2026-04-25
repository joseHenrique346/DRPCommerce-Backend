namespace DropCommerce.Domain.Entity;

public class BaseEntity
{
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
