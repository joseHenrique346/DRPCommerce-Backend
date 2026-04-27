namespace DropCommerce.Domain.StaticEntity;

public sealed class WaitlistEntryStatus : BaseStaticEntity
{
    public static readonly WaitlistEntryStatus Waiting = new(1, "Aguardando");
    public static readonly WaitlistEntryStatus Notified = new(2, "Notificado");
    public static readonly WaitlistEntryStatus Expired = new(3, "Expirado");
    public static readonly WaitlistEntryStatus Fulfilled = new(4, "Atendido");

    private WaitlistEntryStatus(long id, string description) : base(id, description) { }
}