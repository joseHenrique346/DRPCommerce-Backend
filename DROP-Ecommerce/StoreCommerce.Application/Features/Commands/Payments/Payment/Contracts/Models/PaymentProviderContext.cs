using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentProviderContext(string Provider, IntegrationEnvironment Environment, string ExternalAccountId, long? ProviderConnectionId);
