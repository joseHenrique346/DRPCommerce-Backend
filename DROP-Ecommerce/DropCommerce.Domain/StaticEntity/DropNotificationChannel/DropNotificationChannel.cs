namespace DropCommerce.Domain.StaticEntity;

public sealed class DropNotificationChannel : BaseStaticEntity
{
    public static readonly DropNotificationChannel Email = new(1, "E-mail");
    public static readonly DropNotificationChannel SMS = new(2, "SMS");
    public static readonly DropNotificationChannel Push = new(3, "Push");
    public static readonly DropNotificationChannel WhatsApp = new(4, "WhatsApp");

    private DropNotificationChannel(long id, string description) : base(id, description) { }
}