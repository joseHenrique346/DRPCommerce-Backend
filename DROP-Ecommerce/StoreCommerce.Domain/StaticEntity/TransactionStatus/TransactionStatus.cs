namespace StoreCommerce.Domain.StaticEntity;

public sealed class TransactionStatus : BaseStaticEntity
{
    public static readonly TransactionStatus Pending = new(1, "Pendente");
    public static readonly TransactionStatus Authorized = new(2, "Autorizado");
    public static readonly TransactionStatus Captured = new(3, "Capturado");
    public static readonly TransactionStatus Failed = new(4, "Falhou");
    public static readonly TransactionStatus Cancelled = new(5, "Cancelado");
    public static readonly TransactionStatus Refunded = new(6, "Reembolsado");

    private TransactionStatus(long id, string description) : base(id, description) { }
}
