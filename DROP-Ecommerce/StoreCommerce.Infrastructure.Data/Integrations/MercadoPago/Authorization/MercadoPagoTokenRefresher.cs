using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoTokenRefresher(MercadoPagoApi api, IOptions<MercadoPagoOptions> options, TimeProvider time)
    : IProviderTokenRefresher
{
    public string Provider => IntegrationProviders.MercadoPago;

    public async Task<ProviderTokens> RefreshAsync(ProviderConnection connection, ProviderTokens tokens, CancellationToken ct)
    {
        options.Value.RequireEnvironment(connection.Environment);

        if (string.IsNullOrWhiteSpace(options.Value.ClientId) || string.IsNullOrWhiteSpace(options.Value.ClientSecret))
            throw new SecurityFailure(409, "MercadoPago.ConfigurationRequired", "Configure as credenciais OAuth para renovar a autorização.");

        using var response = await api.OAuthAsync(new
        {
            client_id = options.Value.ClientId,
            client_secret = options.Value.ClientSecret,
            grant_type = "refresh_token",
            refresh_token = tokens.RefreshToken
        }, ct);
        var renewed = MercadoPagoTokenResponse.Tokens(response.RootElement, time.GetUtcNow().UtcDateTime, tokens.RefreshToken);
        var account = await api.AccountAsync(renewed.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, connection.Environment);

        if (account.Id != connection.ExternalAccountId || MercadoPagoApi.Id(response.RootElement, "user_id") != account.Id)
            throw new SecurityFailure(409, "MercadoPago.AccountMismatch", "A renovação retornou outra conta.");

        return renewed;
    }
}
