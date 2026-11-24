using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Interfaces.Integrations;

public interface IMercadoPagoAuthorizationClient
{
    string AuthorizationUrl(string state, string verifier);
    Task<MercadoPagoAuthorizedAccount> ExchangeAsync(string code, string verifier, IntegrationEnvironment environment, DateTime now, CancellationToken ct);
    Task<string> SandboxAccountAsync(CancellationToken ct);
}
