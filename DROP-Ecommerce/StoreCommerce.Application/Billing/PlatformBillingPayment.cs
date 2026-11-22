using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Billing;

public sealed record PlatformBillingPayment(string Id, string ExternalReference, string AccountId,
    decimal Amount, string Currency, string Status, decimal RefundedAmount, DateTime UpdatedAt);
