using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using System.Text.Json;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public static class MercadoPagoTokenResponse
{
    public static ProviderTokens Tokens(JsonElement json, DateTime now, string? previousRefreshToken = null)
    {
        var accessToken = MercadoPagoApi.Text(json, "access_token");

        if (string.IsNullOrWhiteSpace(accessToken) || !json.TryGetProperty("expires_in", out var expires) ||
            !expires.TryGetInt64(out var seconds) || seconds is <= 0 or > 31_536_000)
            throw new SecurityFailure(502, "MercadoPago.InvalidTokens", "O provedor retornou credenciais ou validade inválidas.");

        var refresh = MercadoPagoApi.Text(json, "refresh_token");

        return new ProviderTokens(accessToken, string.IsNullOrWhiteSpace(refresh) ? previousRefreshToken : refresh,
            now.AddSeconds(seconds), null);
    }
}
