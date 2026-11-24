using StoreCommerce.Domain.Enums.Integrations;
using System.Text.Json.Serialization;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Integrations;

public sealed record ProviderConnectionInfo(long Id, string Provider, IntegrationEnvironment Environment, string ExternalAccountId, string Scopes, bool IsConnected, DateTime AccessTokenExpiresAt, DateTime? RefreshTokenExpiresAt, DateTime ConnectedAt, DateTime? DisconnectedAt);
