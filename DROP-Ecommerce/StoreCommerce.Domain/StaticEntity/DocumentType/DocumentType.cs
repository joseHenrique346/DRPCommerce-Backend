namespace StoreCommerce.Domain.StaticEntity;

public sealed class DocumentType : BaseStaticEntity
{
    public static readonly DocumentType CPF = new(1, "CPF");
    public static readonly DocumentType CNPJ = new(2, "CNPJ");
    public static readonly DocumentType RG = new(3, "RG");
    public static readonly DocumentType CNH = new(4, "CNH");

    private DocumentType(long id, string description) : base(id, description) { }
}
