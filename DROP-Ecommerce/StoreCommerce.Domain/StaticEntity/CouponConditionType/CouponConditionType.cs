namespace StoreCommerce.Domain.StaticEntity;

/// <summary>
/// O que a condição do cupom mede. Condições do pedido são reavaliadas a cada alteração do carrinho;
/// condições do cliente dependem do histórico de compras e exigem um cliente identificado.
/// </summary>
public sealed class CouponConditionType : BaseStaticEntity
{
    public static readonly CouponConditionType OrderSubTotal = new(1, "Valor do pedido");
    public static readonly CouponConditionType OrderItemQuantity = new(2, "Quantidade de itens do pedido");
    public static readonly CouponConditionType CustomerOrderCount = new(3, "Compras anteriores do cliente");
    public static readonly CouponConditionType CustomerTotalSpent = new(4, "Total já gasto pelo cliente");

    private CouponConditionType(long id, string description) : base(id, description) { }

    public static bool IsCustomerScoped(long id) => id == CustomerOrderCount.Id || id == CustomerTotalSpent.Id;

    public static bool IsMonetary(long id) => id == OrderSubTotal.Id || id == CustomerTotalSpent.Id;
}
