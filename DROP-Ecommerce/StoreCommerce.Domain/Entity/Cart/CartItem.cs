using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Situação do item no momento da consulta.
/// </summary>
public enum CartItemAvailability
{
    /// <summary>Pode ser comprado na quantidade do carrinho.</summary>
    Available = 1,
    /// <summary>Há estoque, mas menos que a quantidade do carrinho.</summary>
    InsufficientStock = 2,
    /// <summary>Sem estoque.</summary>
    OutOfStock = 3,
    /// <summary>Produto ou SKU inativo, excluído ou fora da vitrine.</summary>
    Unavailable = 4
}

public class CartItem : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long CartId { get; private set; }
    public long ProductId { get; private set; }
    public long ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public bool IsSelected { get; private set; }
    public bool IsAwaitingStock { get; private set; }

    #region Navigation Properties
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructor
    protected CartItem() { }
    #endregion

    #region Functions
    internal static CartItem Create(Cart cart, Product product, ProductVariant variant, int quantity) => new()
    {
        Cart = cart,
        CartId = cart.Id,
        EnterpriseId = cart.EnterpriseId,
        Product = product,
        ProductId = product.Id,
        ProductVariant = variant,
        ProductVariantId = variant.Id,
        Quantity = quantity,
        IsSelected = true
    };

    /// <summary>
    /// Preço atual do SKU (o carrinho não congela preço; o valor é o do catálogo no momento da consulta).
    /// </summary>
    public decimal UnitPrice => ProductVariant.Price ?? Product.Price;

    public decimal LineTotal => UnitPrice * Quantity;

    public CartItemAvailability GetAvailability(bool allowBackorder, IReadOnlySet<long> visibleCategoryIds) =>
        Evaluate(Product, ProductVariant, Quantity, allowBackorder, visibleCategoryIds.Contains(Product.CategoryId));

    /// <summary>
    /// Produtos digitais não controlam estoque; com venda sem estoque (allowBackorder) o saldo também não bloqueia.
    /// </summary>
    public static CartItemAvailability Evaluate(Product product, ProductVariant variant, int quantity, bool allowBackorder, bool categoryVisible)
    {
        if (product.IsDeleted || !product.IsActive || !categoryVisible || variant.IsDeleted || !variant.IsActive ||
            (product.HasVariants && variant.IsDefault))
            return CartItemAvailability.Unavailable;

        if (product.IsDigital || allowBackorder || variant.AvailableQuantity >= quantity)
            return CartItemAvailability.Available;

        return variant.AvailableQuantity == 0 ? CartItemAvailability.OutOfStock : CartItemAvailability.InsufficientStock;
    }

    internal void ChangeQuantity(int quantity)
    {
        BaseValidate.ValidateRange(quantity, 1, Cart.MaxQuantityPerItem, nameof(Quantity));

        Quantity = quantity;
    }

    internal void Select()
    {
        IsSelected = true;
        IsAwaitingStock = false;
    }

    internal void Deselect() => IsSelected = false;

    internal void MarkAwaitingStock()
    {
        IsAwaitingStock = true;
        IsSelected = false;
    }
    #endregion
}
