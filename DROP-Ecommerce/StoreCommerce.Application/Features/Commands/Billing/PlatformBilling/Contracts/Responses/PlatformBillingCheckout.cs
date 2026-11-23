using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Billing;

public sealed record PlatformBillingCheckout(string PreferenceId, string CheckoutUrl);
