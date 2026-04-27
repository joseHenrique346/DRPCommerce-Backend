namespace DropCommerce.Domain.StaticEntity;

public sealed class DropEventStatus : BaseStaticEntity
{
    public static readonly DropEventStatus Draft = new(1, "Rascunho");
    public static readonly DropEventStatus RegistrationOpen = new(2, "Inscrições abertas");
    public static readonly DropEventStatus RegistrationClosed = new(3, "Inscrições encerradas");
    public static readonly DropEventStatus QueueOpen = new(4, "Fila aberta");
    public static readonly DropEventStatus Active = new(5, "Ativo");
    public static readonly DropEventStatus SoldOut = new(6, "Esgotado");
    public static readonly DropEventStatus Ended = new(7, "Encerrado");
    public static readonly DropEventStatus Cancelled = new(8, "Cancelado");

    private DropEventStatus(long id, string description) : base(id, description) { }
}