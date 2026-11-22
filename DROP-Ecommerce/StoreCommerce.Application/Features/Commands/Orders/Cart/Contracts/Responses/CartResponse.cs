using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

public sealed record CartResponse(
    long? Id,
    string CurrencyCode,
    IReadOnlyList<CartItemResponse> Items,
    int TotalItems,
    int ItemsRequiringDecision,
    CartSummaryResponse Summary);
