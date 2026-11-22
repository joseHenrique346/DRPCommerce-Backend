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

public sealed record ProviderConnectionInfo(long Id, string Provider, IntegrationEnvironment Environment, string ExternalAccountId, string Scopes, bool IsConnected, DateTime AccessTokenExpiresAt, DateTime? RefreshTokenExpiresAt, DateTime ConnectedAt, DateTime? DisconnectedAt);

public interface IProviderTokenProtector
{
    string Protect(long enterpriseId, string provider, IntegrationEnvironment environment, string accountId, ProviderTokens tokens);
    ProviderTokens Unprotect(ProviderConnection connection);
}

public interface IProviderConnectionStore
{
    Task<ProviderConnection?> GetAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct);
    Task<List<ProviderConnection>> ListAsync(long enterpriseId, CancellationToken ct);
    Task SaveAuthorizationAsync(ProviderConnection connection, CancellationToken ct);
    Task<bool> TryClaimRefreshAsync(ProviderConnection connection, Guid lease, DateTime now, CancellationToken ct);
    Task<bool> CompleteRefreshAsync(ProviderConnection connection, Guid lease, string protectedTokens, ProviderTokens tokens,
        DateTime now, CancellationToken ct);
    Task DisconnectAsync(long enterpriseId, string provider, IntegrationEnvironment environment, DateTime now, CancellationToken ct);
}

/// <summary>
/// Adapter futuro deve obter as validades da resposta do provedor e preservar refresh token quando não houver rotação.
/// </summary>
public interface IProviderTokenRefresher
{
    string Provider { get; }
    Task<ProviderTokens> RefreshAsync(ProviderConnection connection, ProviderTokens tokens, CancellationToken ct);
}

public sealed class ProviderConnectionService(IProviderConnectionStore store, IProviderTokenProtector protector,
    IEnumerable<IProviderTokenRefresher> refreshers, TimeProvider time)
{
    /// <summary>
    /// Ponto de entrada do callback OAuth futuro, após validar state/PKCE e consultar a identidade da conta no provedor.
    /// Não é exposto por HTTP nesta preparação.
    /// </summary>
    public async Task SaveAuthorizedAsync(long enterpriseId, string provider, IntegrationEnvironment environment, string verifiedAccountId, string scopes, ProviderTokens tokens, long authorizedByUserId, CancellationToken ct)
    {
        IntegrationProviders.Validate(provider, environment);
        ValidateTokens(tokens);
        var now = time.GetUtcNow().UtcDateTime;
        var protectedTokens = protector.Protect(enterpriseId, provider, environment, verifiedAccountId, tokens);
        var connection = await store.GetAsync(enterpriseId, provider, environment, ct);

        if (connection is null)
        {
            connection = ProviderConnection.Create(enterpriseId, provider, environment, verifiedAccountId, scopes,
                protectedTokens, tokens.AccessExpiresAt, tokens.RefreshExpiresAt, authorizedByUserId, now);
        }
        else
        {
            connection.Authorize(verifiedAccountId, scopes, protectedTokens, tokens.AccessExpiresAt,
                tokens.RefreshExpiresAt, authorizedByUserId, now);
        }

        await store.SaveAuthorizationAsync(connection, ct);
    }

    public async Task<ProviderTokens> GetTokensAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct)
    {
        IntegrationProviders.Validate(provider, environment);
        var connection = await store.GetAsync(enterpriseId, provider, environment, ct);

        if (connection is not { IsConnected: true })
            throw new SecurityFailure(409, "Integration.Disconnected", "Conecte a conta do provedor nesta loja e ambiente.");

        var now = time.GetUtcNow().UtcDateTime;
        var tokens = protector.Unprotect(connection);

        if (tokens.AccessExpiresAt > now.AddMinutes(1))
            return tokens;

        var refresher = refreshers.SingleOrDefault(item => item.Provider == provider);

        if (refresher is null || string.IsNullOrWhiteSpace(tokens.RefreshToken) || tokens.RefreshExpiresAt <= now)
            throw new SecurityFailure(409, "Integration.ReauthorizationRequired", "Autorize novamente a conta do provedor.");

        var lease = Guid.NewGuid();

        if (!await store.TryClaimRefreshAsync(connection, lease, now, ct))
            throw new SecurityFailure(409, "Integration.RefreshInProgress", "Credenciais em atualização. Tente novamente em instantes.");

        // Uma renovação interrompida pode ter rotacionado o token remoto. A lease permanece até expirar;
        // o adapter deve tratar invalid_grant pedindo nova autorização, sem repetir a troca cegamente.
        using var refreshTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        refreshTimeout.CancelAfter(TimeSpan.FromSeconds(60));
        var renewed = await refresher.RefreshAsync(connection, tokens, refreshTimeout.Token);
        ValidateTokens(renewed);
        var protectedTokens = protector.Protect(enterpriseId, provider, environment, connection.ExternalAccountId, renewed);

        if (!await store.CompleteRefreshAsync(connection, lease, protectedTokens, renewed, time.GetUtcNow().UtcDateTime, ct))
            throw new SecurityFailure(409, "Integration.ConnectionChanged", "A conexão mudou durante a renovação. Consulte novamente.");

        return renewed;
    }

    private void ValidateTokens(ProviderTokens tokens)
    {
        if (string.IsNullOrWhiteSpace(tokens.AccessToken) || tokens.AccessToken.Length > 16000 ||
            tokens.RefreshToken?.Length > 16000 || tokens.AccessExpiresAt <= time.GetUtcNow().UtcDateTime ||
            tokens.RefreshExpiresAt <= time.GetUtcNow().UtcDateTime)
            throw new ArgumentException("Credenciais ou validade retornadas pelo provedor são inválidas.");
    }
}