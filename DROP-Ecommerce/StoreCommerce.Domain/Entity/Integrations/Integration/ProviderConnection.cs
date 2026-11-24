using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Interfaces.Tenancy;
using System.Text.Json.Serialization;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Autorização de uma conta externa para uma loja e ambiente. Credenciais nunca fazem parte de respostas HTTP.
/// A conta é imutável para preservar a identidade de operações financeiras anteriores.
/// </summary>
public sealed class ProviderConnection : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public string ExternalAccountId { get; private set; } = string.Empty;
    public string Scopes { get; private set; } = string.Empty;
    [JsonIgnore]
    public string ProtectedTokens { get; private set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public bool IsConnected { get; private set; }
    public DateTime ConnectedAt { get; private set; }
    public DateTime? DisconnectedAt { get; private set; }
    public DateTime? RefreshedAt { get; private set; }
    public long AuthorizedByUserId { get; private set; }
    public long Version { get; private set; }
    [JsonIgnore]
    public Guid? RefreshLeaseToken { get; private set; }
    public DateTime? RefreshLeaseExpiresAt { get; private set; }

    private ProviderConnection()
    {
    }

    public static ProviderConnection Create(long enterpriseId, string provider, IntegrationEnvironment environment,
        string accountId, string scopes, string protectedTokens, DateTime accessExpiresAt, DateTime? refreshExpiresAt,
        long authorizedByUserId, DateTime now)
    {
        IntegrationProviders.Validate(provider, environment);

        if (enterpriseId <= 0 || authorizedByUserId <= 0 || provider == IntegrationProviders.Manual)
            throw new ArgumentException("Conexão externa inválida.");

        var connection = new ProviderConnection
        {
            EnterpriseId = enterpriseId,
            Provider = provider,
            Environment = environment,
            ExternalAccountId = accountId
        };
        connection.Authorize(accountId, scopes, protectedTokens, accessExpiresAt, refreshExpiresAt, authorizedByUserId, now);

        return connection;
    }

    public void Authorize(string accountId, string scopes, string protectedTokens, DateTime accessExpiresAt,
        DateTime? refreshExpiresAt, long authorizedByUserId, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(accountId) || accountId.Length > 200 || accountId != ExternalAccountId)
            throw new ArgumentException("A conta externa não pode ser substituída nesta conexão.");

        if (string.IsNullOrWhiteSpace(protectedTokens) || scopes.Length > 2000 || authorizedByUserId <= 0 ||
            accessExpiresAt <= now || refreshExpiresAt <= now)
            throw new ArgumentException("Autorização ou validade de credenciais inválida.");

        Scopes = scopes;
        ProtectedTokens = protectedTokens;
        AccessTokenExpiresAt = accessExpiresAt;
        RefreshTokenExpiresAt = refreshExpiresAt;
        AuthorizedByUserId = authorizedByUserId;
        ConnectedAt = now;
        DisconnectedAt = null;
        IsConnected = true;
        RefreshLeaseToken = null;
        RefreshLeaseExpiresAt = null;
        Version++;
    }
}
