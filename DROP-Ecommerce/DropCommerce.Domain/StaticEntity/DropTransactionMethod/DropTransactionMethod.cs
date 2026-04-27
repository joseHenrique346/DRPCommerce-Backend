namespace DropCommerce.Domain.StaticEntity;

public sealed class DropTransactionMethod : BaseStaticEntity
{
    public static readonly DropTransactionMethod CreditCard = new(1, "Cartão de crédito");
    public static readonly DropTransactionMethod Pix = new(2, "Pix");
    public static readonly DropTransactionMethod Boleto = new(3, "Boleto");
    public static readonly DropTransactionMethod Wallet = new(4, "Carteira");

    private DropTransactionMethod(long id, string description) : base(id, description) { }
}