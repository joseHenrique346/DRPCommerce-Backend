namespace SharedCommerce.Domain.StaticEntity;

public sealed class AppearanceRevisionOrigin : BaseStaticEntity
{
    public static readonly AppearanceRevisionOrigin Publish = new(1, "Publish", "Publicação");
    public static readonly AppearanceRevisionOrigin Rollback = new(2, "Rollback", "Restauração");
    public static readonly AppearanceRevisionOrigin Migration = new(3, "Migration", "Migração do tema");

    public string Code { get; }

    private AppearanceRevisionOrigin(long id, string code, string description) : base(id, description)
    {
        Code = code;
    }
}
