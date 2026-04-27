namespace DropCommerce.Domain.StaticEntity;

public sealed class DropNotificationStatus : BaseStaticEntity
{
    public static readonly DropNotificationStatus Scheduled = new(1, "Agendado");
    public static readonly DropNotificationStatus Sent = new(2, "Enviado");
    public static readonly DropNotificationStatus Delivered = new(3, "Entregue");
    public static readonly DropNotificationStatus Failed = new(4, "Falhou");
    public static readonly DropNotificationStatus Bounced = new(5, "Devolvido");

    private DropNotificationStatus(long id, string description) : base(id, description) { }
}