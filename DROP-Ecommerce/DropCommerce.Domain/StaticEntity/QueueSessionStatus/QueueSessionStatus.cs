namespace DropCommerce.Domain.StaticEntity;

public sealed class QueueSessionStatus : BaseStaticEntity
{
    public static readonly QueueSessionStatus Active = new(1, "Ativa");
    public static readonly QueueSessionStatus Expired = new(2, "Expirada");
    public static readonly QueueSessionStatus Invalidated = new(3, "Invalidada");

    private QueueSessionStatus(long id, string description) : base(id, description) { }
}