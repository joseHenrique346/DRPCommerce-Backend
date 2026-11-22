using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Billing;

public sealed record PlatformBillingCharge(string ExternalReference, string AccountId,
    IntegrationEnvironment Environment, decimal Amount, string Currency, DateTime CheckoutExpiresAt);
