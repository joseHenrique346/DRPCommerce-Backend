namespace StoreCommerce.Domain.StaticEntity;

public sealed class InvoiceType : BaseStaticEntity
{
    public static readonly InvoiceType NFe = new(1, "Nota Fiscal Eletrônica");
    public static readonly InvoiceType NFCe = new(2, "Nota Fiscal de Consumidor Eletrônica");
    public static readonly InvoiceType NFSe = new(3, "Nota Fiscal de Serviço Eletrônica");
    public static readonly InvoiceType NF = new(4, "Nota Fiscal");

    private InvoiceType(long id, string description) : base(id, description) { }
}
