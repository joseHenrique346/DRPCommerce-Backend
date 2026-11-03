using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

public sealed record AddCartItemRequest(long ProductId, long? ProductVariantId, int Quantity);

public sealed record UpdateCartItemRequest(int Quantity);

/// <summary>
/// ItemIds nulo ou vazio aplica a todos os itens ("selecionar todos" marca apenas os disponíveis).
/// </summary>
public sealed record CartSelectionRequest(List<long>? ItemIds, bool Selected);

/// <summary>
/// Item do carrinho com estoque e preço atuais. RequiresDecision indica que o item ficou sem estoque ou indisponível
/// e o cliente precisa remover, ajustar a quantidade ou aguardar o estoque. BackInStock indica que um item em espera voltou.
/// </summary>
public sealed record CartItemResponse(
    long Id,
    long ProductId,
    long ProductVariantId,
    string ProductName,
    string ProductSlug,
    string VariantName,
    string VariantAttributesJson,
    string SKU,
    string ImageUrls,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int AvailableQuantity,
    bool TracksStock,
    CartItemAvailability Availability,
    bool IsSelected,
    bool IsAwaitingStock,
    bool RequiresDecision,
    bool BackInStock);

public sealed record CartCouponPreview(string Code, bool IsValid, decimal DiscountAmount, string? ReasonCode, string? Message);

/// <summary>
/// Totais da compra considerando apenas os itens selecionados e disponíveis. CanCheckout resume se a compra pode ser finalizada:
/// há itens selecionados, nenhum selecionado ficou indisponível e o pedido mínimo e o limite de itens da loja são respeitados.
/// </summary>
public sealed record CartSummaryResponse(
    int SelectedItems,
    int SelectedQuantity,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal Total,
    decimal MinOrderValue,
    bool MeetsMinOrderValue,
    int? MaxItemsPerOrder,
    bool WithinItemLimit,
    bool HasUnavailableSelectedItems,
    bool CanCheckout,
    CartCouponPreview? Coupon);

public sealed record CartResponse(
    long? Id,
    string CurrencyCode,
    IReadOnlyList<CartItemResponse> Items,
    int TotalItems,
    int ItemsRequiringDecision,
    CartSummaryResponse Summary);
