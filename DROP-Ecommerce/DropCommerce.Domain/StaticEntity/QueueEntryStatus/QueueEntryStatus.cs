namespace DropCommerce.Domain.StaticEntity;

public sealed class QueueEntryStatus : BaseStaticEntity
{
    public static readonly QueueEntryStatus Waiting = new(1, "Aguardando");
    public static readonly QueueEntryStatus Called = new(2, "Chamado");
    public static readonly QueueEntryStatus CheckingOut = new(3, "Finalizando compra");
    public static readonly QueueEntryStatus Completed = new(4, "Concluído");
    public static readonly QueueEntryStatus Expired = new(5, "Expirado");
    public static readonly QueueEntryStatus Removed = new(6, "Removido");

    private QueueEntryStatus(long id, string description) : base(id, description) { }
}