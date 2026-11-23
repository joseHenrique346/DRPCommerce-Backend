using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StoreCommerce.Infrastructure.Data.Integrations.MelhorEnvio;

public sealed class MelhorEnvioAuthorizationClient(
    HttpClient http,
    IOptions<MelhorEnvioOptions> options,
    TimeProvider time)
    : IMelhorEnvioAuthorizationClient, IProviderTokenRefresher
{
    private MelhorEnvioOptions Settings => options.Value;
    public string Provider => IntegrationProviders.MelhorEnvio;

    public string AuthorizationUrl(IntegrationEnvironment environment, string state)
    {
        var settings = Settings.RequireOAuth(environment);

        return QueryHelpers.AddQueryString(Origin(environment) + "/oauth/authorize", new Dictionary<string, string?>
        {
            ["client_id"] = settings.ClientId.ToString(CultureInfo.InvariantCulture),
            ["response_type"] = "code",
            ["redirect_uri"] = Settings.CallbackUrl(environment),
            ["scope"] = MelhorEnvioOptions.OAuthScopes,
            ["state"] = state
        });
    }

    public async Task<MelhorEnvioAuthorizedAccount> ExchangeAsync(IntegrationEnvironment environment, string code, CancellationToken ct)
    {
        var settings = Settings.RequireOAuth(environment);
        using var request = NewRequest(HttpMethod.Post, environment, "/oauth/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = settings.ClientId.ToString(CultureInfo.InvariantCulture),
            ["client_secret"] = settings.ClientSecret,
            ["redirect_uri"] = Settings.CallbackUrl(environment),
            ["code"] = code
        });

        var tokens = await RequestTokensAsync(request, false, ct);
        var accountId = await GetAccountIdAsync(environment, tokens.AccessToken, ct);

        return new MelhorEnvioAuthorizedAccount(accountId, tokens);
    }

    public async Task<ProviderTokens> RefreshAsync(ProviderConnection connection, ProviderTokens tokens, CancellationToken ct)
    {
        var environment = connection.Environment;
        var settings = Settings.RequireOAuth(environment);
        using var request = NewRequest(HttpMethod.Post, environment, "/oauth/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = settings.ClientId.ToString(CultureInfo.InvariantCulture),
            ["client_secret"] = settings.ClientSecret,
            ["refresh_token"] = tokens.RefreshToken
        });

        var renewed = await RequestTokensAsync(request, true, ct);
        var accountId = await GetAccountIdAsync(environment, renewed.AccessToken, ct);

        if (accountId != connection.ExternalAccountId)
            throw new SecurityFailure(409, "MelhorEnvio.AccountMismatch", "A renovação retornou outra conta do Melhor Envio.");

        return renewed;
    }

    private async Task<ProviderTokens> RequestTokensAsync(HttpRequestMessage request, bool refreshing, CancellationToken ct)
    {
        var requestedAt = time.GetUtcNow().UtcDateTime;
        using var response = await SendAsync(request, ct);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            if (refreshing)
                throw new SecurityFailure(409, "Integration.ReauthorizationRequired", "Autorize novamente a conta do Melhor Envio.");

            throw new SecurityFailure(400, "MelhorEnvio.AuthorizationRejected", "O Melhor Envio recusou a autorização. Inicie uma nova conexão.");
        }

        var payload = await ReadAsync<MelhorEnvioTokenResponse>(response, ct);

        if (!string.Equals(payload.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(payload.AccessToken) || payload.AccessToken.Length > 16000 ||
            payload.AccessToken.Any(char.IsWhiteSpace) || string.IsNullOrWhiteSpace(payload.RefreshToken) ||
            payload.RefreshToken.Length > 16000 || payload.ExpiresIn <= 0)
            throw InvalidResponse();

        return new ProviderTokens(payload.AccessToken, payload.RefreshToken, requestedAt.AddSeconds(payload.ExpiresIn),
            requestedAt.AddDays(45));
    }

    private async Task<string> GetAccountIdAsync(IntegrationEnvironment environment, string accessToken, CancellationToken ct)
    {
        using var request = NewRequest(HttpMethod.Get, environment, "/api/v2/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await SendAsync(request, ct);
        var profile = await ReadAsync<JsonElement>(response, ct);

        if (profile.ValueKind != JsonValueKind.Object || !profile.TryGetProperty("id", out var id))
            throw InvalidResponse();

        var accountId = id.ValueKind switch
        {
            JsonValueKind.String => id.GetString(),
            JsonValueKind.Number when id.TryGetInt64(out var number) && number > 0 => id.GetRawText(),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(accountId) || accountId.Length > 200)
            throw InvalidResponse();

        return accountId;
    }

    private HttpRequestMessage NewRequest(HttpMethod method, IntegrationEnvironment environment, string path)
    {
        var request = new HttpRequestMessage(method, Origin(environment) + path);
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.UserAgent.ParseAdd($"DRPCommerce ({Settings.ContactEmail})");

        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        try
        {
            return await http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new SecurityFailure(502, "MelhorEnvio.Unavailable", "O Melhor Envio está indisponível.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new SecurityFailure(504, "MelhorEnvio.Timeout", "O Melhor Envio não respondeu a tempo.");
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
            throw new SecurityFailure(502, "MelhorEnvio.Unavailable", "O Melhor Envio recusou a consulta.");

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct) ?? throw InvalidResponse();
        }
        catch (JsonException)
        {
            throw InvalidResponse();
        }
    }

    private static string Origin(IntegrationEnvironment environment)
    {
        return environment switch
        {
            IntegrationEnvironment.Sandbox => "https://sandbox.melhorenvio.com.br",
            IntegrationEnvironment.Production => "https://melhorenvio.com.br",
            _ => throw new ArgumentOutOfRangeException(nameof(environment))
        };
    }

    private static SecurityFailure InvalidResponse()
    {
        return new SecurityFailure(502, "MelhorEnvio.InvalidResponse", "O Melhor Envio retornou uma autorização inválida.");
    }
}
