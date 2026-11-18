namespace StoreCommerce.Domain.StaticEntity;

public sealed class TransactionMethod : BaseStaticEntity
{
    public static readonly TransactionMethod CreditCard = new(1, "Cartão de crédito");
    public static readonly TransactionMethod Pix = new(2, "Pix");
    public static readonly TransactionMethod Boleto = new(3, "Boleto");
    public static readonly TransactionMethod MercadoPagoBalance = new(4, "Saldo Mercado Pago");

    private TransactionMethod(long id, string description) : base(id, description) { }
}
