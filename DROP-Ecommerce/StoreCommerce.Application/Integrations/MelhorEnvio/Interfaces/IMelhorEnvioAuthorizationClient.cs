using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Integrations.MelhorEnvio;

public interface IMelhorEnvioAuthorizationClient
{
    string AuthorizationUrl(IntegrationEnvironment environment, string state);
    Task<MelhorEnvioAuthorizedAccount> ExchangeAsync(IntegrationEnvironment environment, string code, CancellationToken ct);
}
