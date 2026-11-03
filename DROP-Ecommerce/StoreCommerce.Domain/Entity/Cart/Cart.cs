using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Carrinho da vitrine: um por cliente, sem expiração e sem reserva de estoque.
/// O estoque é conferido na consulta e na compra; o cliente escolhe quais itens selecionados vai comprar.
/// </summary>
public class Cart : BaseEntity, ITenantEntity
{
    #region Constants
    // Limites técnicos contra abuso; o limite de itens do pedido (StoreSettings) vale apenas na compra.
    public const int MaxDistinctItems = 500;
    public const int MaxQuantityPerItem = 10_000;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public long CustomerId { get; private set; }

    #region Navigation Properties
    public Customer Customer { get; private set; } = null!;
    private readonly List<CartItem> _listCartItem = [];
    public IReadOnlyCollection<CartItem> ListCartItem => _listCartItem.AsReadOnly();
    #endregion

    #endregion

    #region Constructor
    protected Cart() { }
    #endregion

    #region Functions
    public static Cart Create(long enterpriseId, long customerId)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(customerId, nameof(CustomerId));

        return new Cart { EnterpriseId = enterpriseId, CustomerId = customerId };
    }

    /// <summary>
    /// Adiciona o SKU ao carrinho (ou soma a quantidade, se já estiver nele) e deixa o item selecionado.
    /// Só aceita SKUs disponíveis para a quantidade total, já que não há reserva.
    /// </summary>
    public CartItem AddItem(Product product, ProductVariant variant, int quantity, bool allowBackorder)
    {
        BaseValidate.ValidateNotNull(product, nameof(Product));
        BaseValidate.ValidateNotNull(variant, nameof(ProductVariant));
        BaseValidate.ValidatePositive(quantity, nameof(CartItem.Quantity));

        if (product.EnterpriseId != EnterpriseId || variant.EnterpriseId != EnterpriseId || variant.ProductId != product.Id)
            throw new ArgumentException("Produto ou SKU não pertence à loja.");

        var existing = _listCartItem.FirstOrDefault(item => item.ProductVariantId == variant.Id);
        var total = (long)quantity + (existing?.Quantity ?? 0);

        if (total > MaxQuantityPerItem)
            throw new ArgumentException($"A quantidade de um item no carrinho não pode passar de {MaxQuantityPerItem}.");

        if (existing is null && _listCartItem.Count >= MaxDistinctItems)
            throw new ArgumentException($"O carrinho aceita no máximo {MaxDistinctItems} produtos diferentes.");

        var availability = CartItem.Evaluate(product, variant, (int)total, allowBackorder, categoryVisible: true);

        if (availability == CartItemAvailability.Unavailable)
            throw new ArgumentException("Produto ou SKU indisponível para venda.");

        if (availability == CartItemAvailability.OutOfStock)
            throw new ArgumentException("Produto esgotado.");

        if (availability == CartItemAvailability.InsufficientStock)
            throw new ArgumentException($"Estoque insuficiente: há {variant.AvailableQuantity} unidade(s) disponível(is) deste SKU.");

        if (existing is not null)
        {
            existing.ChangeQuantity((int)total);
            existing.Select();

            return existing;
        }

        var item = CartItem.Create(this, product, variant, quantity);
        _listCartItem.Add(item);

        return item;
    }

    /// <summary>
    /// Altera a quantidade. Aumentos exigem estoque disponível; reduções são sempre aceitas, para o cliente resolver falta de estoque.
    /// </summary>
    public CartItem UpdateItemQuantity(long itemId, int quantity, bool allowBackorder)
    {
        BaseValidate.ValidateRange(quantity, 1, MaxQuantityPerItem, nameof(CartItem.Quantity));

        var item = GetItem(itemId);

        if (quantity > item.Quantity &&
            CartItem.Evaluate(item.Product, item.ProductVariant, quantity, allowBackorder, categoryVisible: true) != CartItemAvailability.Available)
            throw new ArgumentException($"Estoque insuficiente: há {item.ProductVariant.AvailableQuantity} unidade(s) disponível(is) deste SKU.");

        item.ChangeQuantity(quantity);

        return item;
    }

    public CartItem RemoveItem(long itemId)
    {
        var item = GetItem(itemId);
        _listCartItem.Remove(item);

        return item;
    }

    public IReadOnlyList<CartItem> Clear()
    {
        var removed = _listCartItem.ToList();
        _listCartItem.Clear();

        return removed;
    }

    /// <summary>
    /// Marca ou desmarca itens para a compra. Só itens disponíveis podem ser marcados; marcar um item que aguardava estoque
    /// encerra a espera.
    /// </summary>
    public void SetSelection(IReadOnlyCollection<long> itemIds, bool selected, bool allowBackorder, IReadOnlySet<long> visibleCategoryIds)
    {
        var items = itemIds.Distinct().Select(GetItem).ToList();

        if (selected)
        {
            var blocked = items.FirstOrDefault(item => item.GetAvailability(allowBackorder, visibleCategoryIds) != CartItemAvailability.Available);

            if (blocked is not null)
                throw new ArgumentException($"O item {blocked.Id} não está disponível para compra. Remova, ajuste a quantidade ou aguarde o estoque.");
        }

        foreach (var item in items)
        {
            if (selected)
                item.Select();
            else
                item.Deselect();
        }
    }

    /// <summary>
    /// "Selecionar todos": marca apenas os itens disponíveis; os demais continuam aguardando a decisão do cliente.
    /// </summary>
    public void SelectAllAvailable(bool allowBackorder, IReadOnlySet<long> visibleCategoryIds)
    {
        foreach (var item in _listCartItem.Where(item => item.GetAvailability(allowBackorder, visibleCategoryIds) == CartItemAvailability.Available))
            item.Select();
    }

    public void DeselectAll()
    {
        foreach (var item in _listCartItem)
            item.Deselect();
    }

    public bool ContainsItem(long itemId) => _listCartItem.Any(item => item.Id == itemId);

    /// <summary>
    /// Mantém no carrinho um item sem estoque (ou indisponível), fora da compra, até o cliente marcá-lo novamente.
    /// </summary>
    public CartItem AwaitStock(long itemId, bool allowBackorder, IReadOnlySet<long> visibleCategoryIds)
    {
        var item = GetItem(itemId);

        if (item.GetAvailability(allowBackorder, visibleCategoryIds) == CartItemAvailability.Available)
            throw new ArgumentException("O item está disponível; não é preciso aguardar estoque.");

        item.MarkAwaitingStock();

        return item;
    }

    private CartItem GetItem(long itemId) =>
        _listCartItem.FirstOrDefault(item => item.Id == itemId)
            ?? throw new ArgumentException($"O item {itemId} não pertence ao carrinho.");
    #endregion
}
