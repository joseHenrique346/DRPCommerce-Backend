using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Enums.Integrations;
using System.Security.Cryptography;
using System.Text;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoAuthorizationClient(
    MercadoPagoApi api,
    IOptions<MercadoPagoOptions> options)
    : IMercadoPagoAuthorizationClient
{
    private MercadoPagoOptions Settings => options.Value;

    public string AuthorizationUrl(string state, string verifier)
    {
        return QueryHelpers.AddQueryString("https://auth.mercadopago.com/authorization", new Dictionary<string, string?>
        {
            ["client_id"] = Settings.ClientId,
            ["response_type"] = "code",
            ["platform_id"] = "mp",
            ["state"] = state,
            ["redirect_uri"] = Settings.OAuthCallbackUrl,
            ["code_challenge"] = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier))),
            ["code_challenge_method"] = "S256"
        });
    }

    public async Task<MercadoPagoAuthorizedAccount> ExchangeAsync(string code, string verifier, IntegrationEnvironment environment, DateTime now, CancellationToken ct)
    {
        using var json = await api.OAuthAsync(new
        {
            client_id = Settings.ClientId,
            client_secret = Settings.ClientSecret,
            grant_type = "authorization_code",
            code,
            redirect_uri = Settings.OAuthCallbackUrl,
            code_verifier = verifier,
            test_token = environment == IntegrationEnvironment.Sandbox
        }, ct);
        var tokens = MercadoPagoTokenResponse.Tokens(json.RootElement, now);
        var account = await api.AccountAsync(tokens.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, environment);

        return new MercadoPagoAuthorizedAccount(account.Id, MercadoPagoApi.Id(json.RootElement, "user_id"), MercadoPagoApi.Text(json.RootElement, "scope"), tokens);
    }

    public async Task<string> SandboxAccountAsync(CancellationToken ct)
    {
        var account = await api.AccountAsync(Settings.SandboxAccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, IntegrationEnvironment.Sandbox);

        return account.Id;
    }
}
