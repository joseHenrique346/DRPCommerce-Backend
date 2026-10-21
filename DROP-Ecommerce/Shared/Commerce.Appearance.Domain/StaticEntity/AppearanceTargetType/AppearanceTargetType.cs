namespace Commerce.Appearance.Domain.StaticEntity;

public sealed class AppearanceTargetType : BaseStaticEntity
{
    public static readonly AppearanceTargetType Store = new(1, "store", "Vitrine");
    public static readonly AppearanceTargetType DropDefault = new(2, "drop-default", "Padrão dos drops");
    public static readonly AppearanceTargetType DropSpecific = new(3, "drop-event", "Drop específico");

    public string Code { get; }

    private AppearanceTargetType(long id, string code, string description) : base(id, description)
    {
        Code = code;
    }
}
