using System.Text.Json.Serialization;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Integrations;

/// <summary>
/// Somente adapters autenticados usam este contrato. Não receber tokens por endpoints de cadastro/CRUD.
/// </summary>
public sealed class ProviderTokens(string accessToken, string? refreshToken, DateTime accessExpiresAt, DateTime? refreshExpiresAt)
{
    [JsonIgnore]
    public string AccessToken { get; } = accessToken;
    [JsonIgnore]
    public string? RefreshToken { get; } = refreshToken;
    public DateTime AccessExpiresAt { get; } = accessExpiresAt;
    public DateTime? RefreshExpiresAt { get; } = refreshExpiresAt;

    public override string ToString()
    {
        return "[Credenciais de integração protegidas]";
    }
}
