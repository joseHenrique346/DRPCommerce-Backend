using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Domain.Enums.Integrations;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations;

public sealed class ProviderTokenProtector(IDataProtectionProvider provider, IOptions<ProviderIntegrationOptions> options)
    : IProviderTokenProtector
{
    public string Protect(long enterpriseId, string providerName, IntegrationEnvironment environment, string accountId,
        ProviderTokens tokens)
    {
        if (string.IsNullOrWhiteSpace(options.Value.KeyRingPath))
            throw new InvalidOperationException("Configure ProviderIntegrations:KeyRingPath antes de conectar contas externas.");

        return CreateProtector(enterpriseId, providerName, environment, accountId).Protect(JsonSerializer.Serialize(
            new TokenPayload(tokens.AccessToken, tokens.RefreshToken, tokens.AccessExpiresAt, tokens.RefreshExpiresAt)));
    }

    public ProviderTokens Unprotect(ProviderConnection connection)
    {
        var json = CreateProtector(connection.EnterpriseId, connection.Provider, connection.Environment,
            connection.ExternalAccountId).Unprotect(connection.ProtectedTokens);
        var payload = JsonSerializer.Deserialize<TokenPayload>(json)
            ?? throw new InvalidOperationException("Credenciais de integração inválidas.");

        return new ProviderTokens(payload.AccessToken, payload.RefreshToken, payload.AccessExpiresAt, payload.RefreshExpiresAt);
    }

    private IDataProtector CreateProtector(long enterpriseId, string providerName, IntegrationEnvironment environment, string accountId)
    {
        return provider.CreateProtector("StoreCommerce.ProviderTokens.v1", enterpriseId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            providerName, environment.ToString(), accountId);
    }

    private sealed record TokenPayload(string AccessToken, string? RefreshToken, DateTime AccessExpiresAt, DateTime? RefreshExpiresAt);
}
