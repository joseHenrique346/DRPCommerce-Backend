namespace DropCommerce.Domain.StaticEntity;

public sealed class DropNotificationType : BaseStaticEntity
{
    public static readonly DropNotificationType RegistrationConfirmed = new(1, "Inscrição confirmada");
    public static readonly DropNotificationType QueueOpening = new(2, "Abertura da fila");
    public static readonly DropNotificationType QueueCalled = new(3, "Chamado na fila");
    public static readonly DropNotificationType ReservationExpiring = new(4, "Reserva expirando");
    public static readonly DropNotificationType OrderConfirmed = new(5, "Pedido confirmado");
    public static readonly DropNotificationType WaitlistAvailable = new(6, "Disponível na lista de espera");

    private DropNotificationType(long id, string description) : base(id, description) { }
}