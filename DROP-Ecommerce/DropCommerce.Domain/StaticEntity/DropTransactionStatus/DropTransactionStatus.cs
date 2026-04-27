namespace DropCommerce.Domain.StaticEntity;

public sealed class DropTransactionStatus : BaseStaticEntity
{
    public static readonly DropTransactionStatus Pending = new(1, "Pendente");
    public static readonly DropTransactionStatus Authorized = new(2, "Autorizado");
    public static readonly DropTransactionStatus Captured = new(3, "Capturado");
    public static readonly DropTransactionStatus Failed = new(4, "Falhou");
    public static readonly DropTransactionStatus Cancelled = new(5, "Cancelado");
    public static readonly DropTransactionStatus Refunded = new(6, "Reembolsado");

    private DropTransactionStatus(long id, string description) : base(id, description) { }
}