namespace Commerce.Appearance.Domain.StaticEntity;

public sealed class AppearanceChannel : BaseStaticEntity
{
    public static readonly AppearanceChannel Store = new(1, "store", "Vitrine");
    public static readonly AppearanceChannel Drop = new(2, "drop", "Drop");

    public string Code { get; }

    private AppearanceChannel(long id, string code, string description) : base(id, description)
    {
        Code = code;
    }
}
