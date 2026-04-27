namespace DropCommerce.Domain.StaticEntity;

public sealed class DropRegistrationStatus : BaseStaticEntity
{
    public static readonly DropRegistrationStatus Pending = new(1, "Pendente");
    public static readonly DropRegistrationStatus Eligible = new(2, "Elegível");
    public static readonly DropRegistrationStatus Ineligible = new(3, "Inelegível");
    public static readonly DropRegistrationStatus Waitlisted = new(4, "Na lista de espera");

    private DropRegistrationStatus(long id, string description) : base(id, description) { }
}