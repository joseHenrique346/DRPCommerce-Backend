namespace StoreCommerce.Application.Payments;

public sealed record DropSaleGatewayResult(string Status, string? PaymentId);
