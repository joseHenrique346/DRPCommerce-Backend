using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Integrations.Providers.Interfaces;

public interface IProviderConnectionCredentials
{
    Task SaveAuthorizedAsync(long enterpriseId, string provider, IntegrationEnvironment environment,
        string verifiedAccountId, string scopes, ProviderTokens tokens, long authorizedByUserId, CancellationToken ct);
    Task<ProviderTokens> GetTokensAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct);
}
