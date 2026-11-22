using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoAuthorizationService(StoreCommerceDbContext context, ICurrentIdentity actor,
    MercadoPagoApi api, ProviderConnectionService connections, IPaymentGatewayRegistry gateways,
    IDataProtectionProvider protection, IntegrationWriteScope scope, IOptions<MercadoPagoOptions> options,
    IOptions<ProviderIntegrationOptions> integrationOptions, TimeProvider time)
{
    private MercadoPagoOptions Settings => options.Value;

    public async Task<MercadoPagoAuthorizationStart> BeginAsync(IntegrationEnvironment environment, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        Settings.RequireOAuth(environment);
        RequireKeyRing();
        var version = await RequireAccessAsync(actor.EnterpriseId, actor.UserId, actor.MembershipId, null, ct);
        var state = Random();
        var browser = Random();
        var verifier = Random();
        var authorization = ProviderAuthorization.Create(actor.EnterpriseId, actor.UserId, actor.MembershipId, version,
            Hash(state), Hash(browser), Protector().Protect(verifier), environment, time.GetUtcNow().UtcDateTime);
        context.Add(authorization);

        using (scope.Begin(actor.EnterpriseId))
            await context.SaveChangesAsync(ct);

        var url = QueryHelpers.AddQueryString("https://auth.mercadopago.com/authorization", new Dictionary<string, string?>
        {
            ["client_id"] = Settings.ClientId,
            ["response_type"] = "code",
            ["platform_id"] = "mp",
            ["state"] = state,
            ["redirect_uri"] = Settings.OAuthCallbackUrl,
            ["code_challenge"] = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier))),
            ["code_challenge_method"] = "S256"
        });

        return new MercadoPagoAuthorizationStart(url, browser, authorization.ExpiresAt);
    }

    public async Task CompleteAsync(string state, string browserBinding, string? code, string? error, CancellationToken ct)
    {
        if (state.Length is < 20 or > 200 || browserBinding.Length is < 20 or > 200)
            throw InvalidState();

        var hash = Hash(state);
        var browserHash = Hash(browserBinding);
        var now = time.GetUtcNow().UtcDateTime;
        var authorization = await context.Set<ProviderAuthorization>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Authorization => Authorization.StateHash == hash && Authorization.BrowserHash == browserHash &&
                Authorization.ConsumedAt == null && Authorization.ExpiresAt > now, ct)
            ?? throw InvalidState();
        Settings.RequireOAuth(authorization.Environment);
        RequireKeyRing();
        var consumed = await context.Set<ProviderAuthorization>().IgnoreQueryFilters()
            .Where(Authorization => Authorization.Id == authorization.Id && Authorization.ConsumedAt == null && Authorization.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(Authorization => Authorization.ConsumedAt, now)
                .SetProperty(Authorization => Authorization.ProtectedVerifier, string.Empty), ct);

        if (consumed != 1)
            throw InvalidState();

        if (!string.IsNullOrWhiteSpace(error) || string.IsNullOrWhiteSpace(code) || code.Length > 2000)
            throw new SecurityFailure(400, "MercadoPago.AuthorizationDenied", "A autorização não foi concluída. Inicie uma nova conexão.");

        await RequireAccessAsync(authorization.EnterpriseId, authorization.UserId, authorization.MembershipId, authorization.UserVersion, ct);
        using var json = await api.OAuthAsync(new
        {
            client_id = Settings.ClientId,
            client_secret = Settings.ClientSecret,
            grant_type = "authorization_code",
            code,
            redirect_uri = Settings.OAuthCallbackUrl,
            code_verifier = Protector().Unprotect(authorization.ProtectedVerifier),
            test_token = authorization.Environment == IntegrationEnvironment.Sandbox
        }, ct);
        var tokens = Tokens(json.RootElement, now);
        var account = await api.AccountAsync(tokens.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, authorization.Environment);

        if (MercadoPagoApi.Id(json.RootElement, "user_id") != account.Id)
            throw new SecurityFailure(409, "MercadoPago.AccountMismatch", "O token e o vendedor autorizado não correspondem.");

        // A rede ficou fora desta transação. Revalida o acesso atual, inclusive exclusão/revogação ocorrida no provedor.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await RequireAccessAsync(authorization.EnterpriseId, authorization.UserId, authorization.MembershipId, authorization.UserVersion, ct);
        await connections.SaveAuthorizedAsync(authorization.EnterpriseId, IntegrationProviders.MercadoPago, authorization.Environment,
            account.Id, MercadoPagoApi.Text(json.RootElement, "scope"), tokens, authorization.UserId, ct);
        await gateways.SelectAsync(authorization.EnterpriseId, IntegrationProviders.MercadoPago, authorization.Environment, ct);
        await transaction.CommitAsync(ct);
    }

    public async Task ConnectSandboxAsync(CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        Settings.RequireCheckout(IntegrationEnvironment.Sandbox);
        RequireKeyRing();

        if (string.IsNullOrWhiteSpace(Settings.SandboxAccessToken))
            throw new SecurityFailure(409, "MercadoPago.SandboxTokenRequired", "Configure o token do vendedor de teste no servidor.");

        var account = await api.AccountAsync(Settings.SandboxAccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, IntegrationEnvironment.Sandbox);
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await RequireAccessAsync(actor.EnterpriseId, actor.UserId, actor.MembershipId, null, ct);
        // Prazo local curto para uma chave de teste estática; não inventa validade remota nem refresh token.
        await connections.SaveAuthorizedAsync(actor.EnterpriseId, IntegrationProviders.MercadoPago, IntegrationEnvironment.Sandbox,
            account.Id, "sandbox-static", new ProviderTokens(Settings.SandboxAccessToken, null,
                time.GetUtcNow().UtcDateTime.AddHours(1), null), actor.UserId, ct);
        await gateways.SelectAsync(actor.EnterpriseId, IntegrationProviders.MercadoPago, IntegrationEnvironment.Sandbox, ct);
        await transaction.CommitAsync(ct);
    }

    private async Task<long> RequireAccessAsync(long enterpriseId, long userId, long membershipId, long? expectedVersion, CancellationToken ct)
    {
        var membership = await context.EnterpriseMemberships.IgnoreQueryFilters().AsNoTracking()
            .Include(Membership => Membership.User).Include(Membership => Membership.Enterprise)
            .SingleOrDefaultAsync(Membership => Membership.Id == membershipId && Membership.EnterpriseId == enterpriseId &&
                Membership.UserId == userId && Membership.IsActive, ct);

        if (membership is null || !membership.User.IsActive || !membership.Enterprise.IsActive ||
            expectedVersion.HasValue && membership.User.SecurityVersion != expectedVersion)
            throw new SecurityFailure(403, "MercadoPago.AuthorizationRevoked", "O acesso à loja mudou. Inicie uma nova autorização.");

        var allowed = membership.KindId == EnterpriseMembershipKind.PrimaryOwner.Id ||
            await context.MembershipAccessRoles.IgnoreQueryFilters().AnyAsync(Assignment =>
                Assignment.EnterpriseId == enterpriseId && Assignment.EnterpriseMembershipId == membershipId && Assignment.AccessRole.IsActive &&
                Assignment.AccessRole.ListAccessRolePermission.Any(Grant => Grant.Permission.IsActive &&
                    Grant.Permission.Code == PermissionCodes.SettingsManage), ct);

        if (!allowed)
            throw new SecurityFailure(403, "Security.PermissionRequired", "Permissão settings.manage necessária.");

        return membership.User.SecurityVersion;
    }

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

    private IDataProtector Protector()
    {
        return protection.CreateProtector("MercadoPago.OAuth.Pkce.v1");
    }

    private void RequireKeyRing()
    {
        if (string.IsNullOrWhiteSpace(integrationOptions.Value.KeyRingPath))
            throw new SecurityFailure(409, "Integration.KeyRingRequired", "Configure o armazenamento persistente das chaves de integração.");
    }

    public static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string Random()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    private static SecurityFailure InvalidState()
    {
        return new SecurityFailure(400, "MercadoPago.InvalidState", "A autorização expirou, já foi usada ou pertence a outro navegador.");
    }
}
