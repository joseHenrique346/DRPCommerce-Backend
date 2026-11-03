namespace StoreCommerce.Domain.Entity;

public class BaseEntity
{
    #region Properties
    public long Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Toda entidade nasce com a data de criação em UTC. Ao carregar do banco, o EF sobrescreve com o valor gravado.
    /// A data é truncada em microssegundos, a mesma precisão do PostgreSQL, para não mudar depois de salva.
    /// </summary>
    protected BaseEntity()
    {
        var now = DateTime.UtcNow;
        CreatedAt = new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
    }
    #endregion

    #region Functions
    public void SetChangedDate()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}