using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class MelhorEnvioOAuthTests(MelhorEnvioOAuthFixture fixture) : IClassFixture<MelhorEnvioOAuthFixture>
{
    [PostgresFact]
    public async Task Callback_is_bound_to_provider_environment_and_browser_and_can_only_be_used_once()
    {
        fixture.Provider.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);

        using var wrongProvider = await CallbackAsync("/api/integrations/mercado-pago/oauth/callback",
            start.State, "__Host-mp-oauth=" + start.Binding);
        using var wrongEnvironment = await CallbackAsync("/api/integrations/melhor-envio/oauth/production/callback",
            start.State, "__Host-me-oauth-production=" + start.Binding);
        using var wrongBrowser = await CallbackAsync(start.CallbackPath, start.State,
            "__Host-me-oauth-sandbox=another-browser-binding-12345");

        Assert.Equal(HttpStatusCode.BadRequest, wrongProvider.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, wrongEnvironment.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, wrongBrowser.StatusCode);
        Assert.Equal(0, fixture.Provider.ExchangeCalls);

        using var first = await CallbackAsync(start.CallbackPath, start.State, start.Cookie);
        using var replay = await CallbackAsync(start.CallbackPath, start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Redirect, first.StatusCode);
        Assert.Equal("https://backoffice.example.invalid/integrations?melhorEnvio=connected", first.Headers.Location?.AbsoluteUri);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(1, fixture.Provider.ExchangeCalls);

        using var scope = fixture.Store.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var connection = await db.ProviderConnections.IgnoreQueryFilters()
            .SingleAsync(Connection => Connection.EnterpriseId == owner.EnterpriseId);
        Assert.Equal(IntegrationProviders.MelhorEnvio, connection.Provider);
        Assert.Equal(IntegrationEnvironment.Sandbox, connection.Environment);
        Assert.Equal("314", connection.ExternalAccountId);
        Assert.Equal(MelhorEnvioOptions.OAuthScopes, connection.Scopes);
        Assert.True(connection.IsConnected);
        Assert.DoesNotContain("test-only-access", connection.ProtectedTokens);
        Assert.DoesNotContain("test-only-refresh", connection.ProtectedTokens);
        Assert.False(await db.Set<PaymentProviderSelection>().IgnoreQueryFilters()
            .AnyAsync(Selection => Selection.EnterpriseId == owner.EnterpriseId));

        var authorization = await db.Set<ProviderAuthorization>().IgnoreQueryFilters()
            .SingleAsync(Authorization => Authorization.EnterpriseId == owner.EnterpriseId);
        Assert.Equal(IntegrationProviders.MelhorEnvio, authorization.Provider);
        Assert.Equal(IntegrationEnvironment.Sandbox, authorization.Environment);
        Assert.NotEqual(start.State, authorization.StateHash);
        Assert.NotEqual(start.Binding, authorization.BrowserHash);
        Assert.NotNull(authorization.ConsumedAt);
    }

    [PostgresFact]
    public async Task Access_revoked_during_code_exchange_prevents_connection()
    {
        fixture.Provider.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        fixture.Provider.DuringExchange = async () =>
        {
            using var scope = fixture.Store.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            await db.Users.IgnoreQueryFilters().Where(User => User.Id == owner.UserId)
                .ExecuteUpdateAsync(Setters => Setters.SetProperty(User => User.IsActive, false));
        };

        using var callback = await CallbackAsync(start.CallbackPath, start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Forbidden, callback.StatusCode);
        using var problem = JsonDocument.Parse(await callback.Content.ReadAsStringAsync());
        Assert.Equal("MelhorEnvio.AuthorizationRevoked", problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(1, fixture.Provider.ExchangeCalls);

        using var verification = fixture.Store.Services.CreateScope();
        var database = verification.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.False(await database.ProviderConnections.IgnoreQueryFilters()
            .AnyAsync(Connection => Connection.EnterpriseId == owner.EnterpriseId));
    }

    [PostgresFact]
    public async Task Refresh_rotates_protected_credentials_for_the_verified_account()
    {
        fixture.Provider.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        using var callback = await CallbackAsync(start.CallbackPath, start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);

        using var scope = fixture.Store.Services.CreateScope();
        var credentials = scope.ServiceProvider.GetRequiredService<IProviderConnectionCredentials>();
        var renewed = await credentials.GetTokensAsync(owner.EnterpriseId, IntegrationProviders.MelhorEnvio,
            IntegrationEnvironment.Sandbox, CancellationToken.None);
        var stored = await credentials.GetTokensAsync(owner.EnterpriseId, IntegrationProviders.MelhorEnvio,
            IntegrationEnvironment.Sandbox, CancellationToken.None);

        Assert.Equal(1, fixture.Provider.RefreshCalls);
        Assert.Equal("test-only-access-renewed", stored.AccessToken);
        Assert.Equal("test-only-refresh-renewed", stored.RefreshToken);
        Assert.Equal(renewed.AccessToken, stored.AccessToken);
        Assert.Equal(renewed.RefreshToken, stored.RefreshToken);
        Assert.Equal(renewed.AccessExpiresAt, stored.AccessExpiresAt);
        Assert.Equal(renewed.RefreshExpiresAt, stored.RefreshExpiresAt);
    }

    private async Task<(string State, string Cookie, string Binding, string CallbackPath)> BeginAsync(StoreActor actor)
    {
        using var client = fixture.Store.Client(actor.Token);
        using var response = await client.PostAsJsonAsync("/api/integrations/melhor-envio/oauth/start",
            new { environment = IntegrationEnvironment.Sandbox });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var url = new Uri(json.RootElement.GetProperty("authorizationUrl").GetString()!);
        var query = QueryHelpers.ParseQuery(url.Query);
        Assert.Equal("sandbox.melhorenvio.com.br", url.Host);
        Assert.Equal("/oauth/authorize", url.AbsolutePath);
        Assert.Equal("123", query["client_id"].ToString());
        Assert.Equal(MelhorEnvioOptions.OAuthScopes, query["scope"].ToString());
        var callback = new Uri(query["redirect_uri"].ToString());
        Assert.Equal("https://api.example.invalid/api/integrations/melhor-envio/oauth/sandbox/callback", callback.AbsoluteUri);

        var setCookie = response.Headers.GetValues("Set-Cookie").Single();
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
        var cookie = setCookie.Split(';')[0];
        Assert.StartsWith("__Host-me-oauth-sandbox=", cookie);
        var binding = cookie[(cookie.IndexOf('=') + 1)..];
        Assert.DoesNotContain(binding, json.RootElement.GetRawText());

        return (query["state"].ToString(), cookie, binding, callback.AbsolutePath);
    }

    private async Task<HttpResponseMessage> CallbackAsync(string path, string state, string cookie)
    {
        using var client = fixture.Store.Client();
        using var request = new HttpRequestMessage(HttpMethod.Get, path + "?state=" + Uri.EscapeDataString(state) + "&code=test-code");
        request.Headers.Add("Cookie", cookie);

        return await client.SendAsync(request);
    }
}
