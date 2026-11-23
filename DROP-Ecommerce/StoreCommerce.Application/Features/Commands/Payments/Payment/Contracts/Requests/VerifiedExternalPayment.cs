using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

public sealed record VerifiedExternalPayment(long EnterpriseId, string Provider, IntegrationEnvironment Environment,
    string ExternalAccountId, string ExternalReference, string PaymentId, decimal Amount, string CurrencyCode);
