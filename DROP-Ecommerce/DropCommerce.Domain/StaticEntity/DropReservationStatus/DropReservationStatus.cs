namespace DropCommerce.Domain.StaticEntity;

public sealed class DropReservationStatus : BaseStaticEntity
{
    public static readonly DropReservationStatus Active = new(1, "Ativa");
    public static readonly DropReservationStatus Confirmed = new(2, "Confirmada");
    public static readonly DropReservationStatus Expired = new(3, "Expirada");
    public static readonly DropReservationStatus Cancelled = new(4, "Cancelada");

    private DropReservationStatus(long id, string description) : base(id, description) { }
}