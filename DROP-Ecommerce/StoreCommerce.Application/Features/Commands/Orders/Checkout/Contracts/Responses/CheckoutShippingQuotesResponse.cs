namespace StoreCommerce.Application.Features.Commands;

public sealed record CheckoutShippingQuotesResponse(bool RequiresQuote, bool AllowsPickup, string CurrencyCode,
    DateTime? ExpiresAt, IReadOnlyList<CheckoutShippingOptionResponse> Options);
