namespace Commerce.Appearance.Domain.StaticEntity;

public sealed class AppearanceEditSessionStatus : BaseStaticEntity
{
    public static readonly AppearanceEditSessionStatus InEdit = new(1, "InEdit", "Em edição");
    public static readonly AppearanceEditSessionStatus InPreview = new(2, "InPreview", "Em prévia");
    public static readonly AppearanceEditSessionStatus Published = new(3, "Published", "Publicada");
    public static readonly AppearanceEditSessionStatus Cancelled = new(4, "Cancelled", "Cancelada");
    public static readonly AppearanceEditSessionStatus Released = new(5, "Released", "Liberada pelo proprietário");
    public static readonly AppearanceEditSessionStatus Expired = new(6, "Expired", "Expirada");
    public static readonly AppearanceEditSessionStatus Migrated = new(7, "Migrated", "Encerrada por migração");

    public string Code { get; }

    private AppearanceEditSessionStatus(long id, string code, string description) : base(id, description)
    {
        Code = code;
    }
}
