using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record MercadoPagoConnectionSummary(long Id, IntegrationEnvironment Environment, string ExternalAccountId, bool IsConnected, DateTime AccessTokenExpiresAt, DateTime ConnectedAt, DateTime? DisconnectedAt, string WebhookUrl);
