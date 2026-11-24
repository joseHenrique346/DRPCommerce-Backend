using StoreCommerce.Application.Integrations;
using System.Text.Json.Serialization;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Integrations.Providers.Interfaces;

/// <summary>
/// Adapter futuro deve obter as validades da resposta do provedor e preservar refresh token quando não houver rotação.
/// </summary>
public interface IProviderTokenRefresher
{
    string Provider { get; }
    Task<ProviderTokens> RefreshAsync(ProviderConnection connection, ProviderTokens tokens, CancellationToken ct);
}
