using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Domain.Entity;

public sealed record OrderShippingSnapshot(
    string Provider, IntegrationEnvironment Environment, long ConnectionId, DateTime ConnectedAt,
    string OriginPostalCode, string DestinationPostalCode, DateTime QuotedAt, DateTime ExpiresAt,
    ShippingServiceQuote Service);
