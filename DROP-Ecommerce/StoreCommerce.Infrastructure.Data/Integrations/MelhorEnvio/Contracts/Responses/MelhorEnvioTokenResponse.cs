using System.Text.Json.Serialization;

namespace StoreCommerce.Infrastructure.Data.Integrations.MelhorEnvio;

internal sealed record MelhorEnvioTokenResponse(
    [property: JsonPropertyName("token_type")] string? TokenType,
    [property: JsonPropertyName("access_token")] string? AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
