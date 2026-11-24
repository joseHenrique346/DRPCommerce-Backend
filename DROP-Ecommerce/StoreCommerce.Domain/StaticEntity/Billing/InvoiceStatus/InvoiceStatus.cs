namespace StoreCommerce.Domain.StaticEntity;

public sealed class InvoiceStatus : BaseStaticEntity
{
    public static readonly InvoiceStatus Pending = new(1, "Pendente");
    public static readonly InvoiceStatus Authorized = new(2, "Autorizada");
    public static readonly InvoiceStatus Issued = new(3, "Emitida");
    public static readonly InvoiceStatus Cancelled = new(4, "Cancelada");

    private InvoiceStatus(long id, string description) : base(id, description) { }
}
