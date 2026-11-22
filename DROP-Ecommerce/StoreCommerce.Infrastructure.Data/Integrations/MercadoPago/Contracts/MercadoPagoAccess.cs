using StoreCommerce.Application.Integrations.Providers.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

/// <summary>
/// Endereço fixo e erros sem corpo/payload do provedor. Não há retry automático de escritas.
/// </summary>
public sealed class MercadoPagoAccess(MercadoPagoApi api, IProviderConnectionStore connections,
    ProviderConnectionService credentials)
{
    public async Task<string> AccessAsync(long enterpriseId, PaymentProviderContext snapshot, CancellationToken ct)
    {
        var connection = await connections.GetAsync(enterpriseId, snapshot.Provider, snapshot.Environment, ct);

        if (snapshot.Provider != IntegrationProviders.MercadoPago || connection is not { IsConnected: true } ||
            connection.Id != snapshot.ProviderConnectionId || connection.ExternalAccountId != snapshot.ExternalAccountId)
            throw new SecurityFailure(409, "MercadoPago.ConnectionChanged", "Autorize novamente a conta original deste pagamento.");

        var tokens = await credentials.GetTokensAsync(enterpriseId, snapshot.Provider, snapshot.Environment, ct);
        var account = await api.AccountAsync(tokens.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, snapshot.Environment);

        if (account.Id != snapshot.ExternalAccountId)
            throw new SecurityFailure(409, "MercadoPago.AccountMismatch", "A credencial não pertence ao recebedor original.");

        return tokens.AccessToken;
    }
}
