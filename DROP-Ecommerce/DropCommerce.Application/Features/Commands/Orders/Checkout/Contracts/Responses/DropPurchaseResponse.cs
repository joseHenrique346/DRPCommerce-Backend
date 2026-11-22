namespace DropCommerce.Application.Contracts;

public sealed record DropPurchaseResponse(
    long Id,
    long DropEventId,
    long DropReservationId,
    long DropOrderStatusId,
    long DropOrderPaymentStatusId,
    decimal TotalAmount,
    string? BuyerNameSnapshot,
    string? BuyerEmailSnapshot,
    string? PaymentCheckoutUrl,
    string? FinancialReviewCode,
    string ShippingAddressLine,
    string ShippingCity,
    string ShippingState,
    string ShippingZipCode,
    IEnumerable<DropPurchaseItemResponse> Items);
