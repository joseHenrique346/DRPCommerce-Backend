namespace StoreCommerce.Application.Integrations.MercadoPago;

public sealed record MercadoPagoPayment(string Id, string ExternalReference, string CollectorId, decimal Amount,
    string CurrencyCode, string Status, decimal RefundedAmount, DateTime UpdatedAt, string MethodId, string MethodType);
