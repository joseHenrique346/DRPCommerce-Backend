using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

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
