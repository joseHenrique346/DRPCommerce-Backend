namespace StoreCommerce.Domain.StaticEntity;

public sealed class EnterpriseMembershipKind : BaseStaticEntity
{
    public static readonly EnterpriseMembershipKind PrimaryOwner = new(1, "Dono principal");
    public static readonly EnterpriseMembershipKind CoOwner = new(2, "Socio-proprietario");
    public static readonly EnterpriseMembershipKind Staff = new(3, "Funcionario");

    private EnterpriseMembershipKind(long id, string description) : base(id, description) { }
}
