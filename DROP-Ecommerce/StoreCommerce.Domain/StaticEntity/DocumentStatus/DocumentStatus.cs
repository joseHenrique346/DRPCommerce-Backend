namespace StoreCommerce.Domain.StaticEntity;

public sealed class DocumentStatus : BaseStaticEntity
{
    public static readonly DocumentStatus Pending = new(1, "Pendente");
    public static readonly DocumentStatus AwaitingValidation = new(2, "Aguardando validação");
    public static readonly DocumentStatus Validated = new(3, "Validado");
    public static readonly DocumentStatus Rejected = new(4, "Rejeitado");
    public static readonly DocumentStatus Expired = new(5, "Expirado");

    private DocumentStatus(long id, string description) : base(id, description) { }
}
