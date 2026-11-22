using StoreCommerce.Application.Integrations;
using StoreCommerce.Domain.Enums.Integrations;
using System.Text.Json.Serialization;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Integrations.Providers.Interfaces;

public interface IProviderConnectionStore
{
    Task<ProviderConnection?> GetAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct);
    Task<List<ProviderConnection>> ListAsync(long enterpriseId, CancellationToken ct);
    Task SaveAuthorizationAsync(ProviderConnection connection, CancellationToken ct);
    Task<bool> TryClaimRefreshAsync(ProviderConnection connection, Guid lease, DateTime now, CancellationToken ct);
    Task<bool> CompleteRefreshAsync(ProviderConnection connection, Guid lease, string protectedTokens, ProviderTokens tokens,
        DateTime now, CancellationToken ct);
    Task DisconnectAsync(long enterpriseId, string provider, IntegrationEnvironment environment, DateTime now, CancellationToken ct);
}
