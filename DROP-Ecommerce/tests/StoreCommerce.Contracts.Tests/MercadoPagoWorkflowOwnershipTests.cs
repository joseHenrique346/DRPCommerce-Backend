using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Interfaces.Integrations;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class MercadoPagoWorkflowOwnershipTests(MercadoPagoWorkflowFixture fixture) : IClassFixture<MercadoPagoWorkflowFixture>
{
    [PostgresFact]
    public async Task OAuth_binding_and_single_use_are_preserved_without_exposing_credentials()
    {
        fixture.Client.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        using var wrongBrowser = await CallbackAsync(start.State, "__Host-mp-oauth=another-browser-binding-12345");
        Assert.Equal(HttpStatusCode.BadRequest, wrongBrowser.StatusCode);
        Assert.Equal(0, fixture.Client.ExchangeCalls);

        using var completed = await CallbackAsync(start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Redirect, completed.StatusCode);
        Assert.Equal(1, fixture.Client.ExchangeCalls);
        using var replay = await CallbackAsync(start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(1, fixture.Client.ExchangeCalls);

        using var scope = fixture.Store.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var authorization = await db.Set<StoreCommerce.Domain.Entity.ProviderAuthorization>().IgnoreQueryFilters()
            .SingleAsync(Authorization => Authorization.EnterpriseId == owner.EnterpriseId);
        Assert.NotNull(authorization.ConsumedAt);
        Assert.Empty(authorization.ProtectedVerifier);
        Assert.NotEqual(start.State, authorization.StateHash);
        var connection = await db.ProviderConnections.IgnoreQueryFilters().SingleAsync(Connection => Connection.EnterpriseId == owner.EnterpriseId);
        Assert.True(connection.IsConnected);
        Assert.DoesNotContain("test-fake-access", connection.ProtectedTokens);

        using var client = fixture.Store.Client(owner.Token);
        using var response = await client.GetAsync("/api/integrations/mercado-pago");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("test-fake-access", text);
        Assert.DoesNotContain("protectedTokens", text);
        using var json = JsonDocument.Parse(text);
        Assert.Equal(IntegrationProviders.MercadoPago, json.RootElement.GetProperty("selectedProvider").GetString());
        Assert.Single(json.RootElement.GetProperty("connections").EnumerateArray());
    }

    [PostgresFact]
    public async Task Concurrent_callbacks_exchange_the_code_once()
    {
        fixture.Client.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        var callbacks = await Task.WhenAll(CallbackAsync(start.State, start.Cookie), CallbackAsync(start.State, start.Cookie));

        try
        {
            Assert.Equal(1, callbacks.Count(Response => Response.StatusCode == HttpStatusCode.Redirect));
            Assert.Equal(1, callbacks.Count(Response => Response.StatusCode == HttpStatusCode.BadRequest));
            Assert.Equal(1, fixture.Client.ExchangeCalls);
        }
        finally
        {
            foreach (var callback in callbacks)
                callback.Dispose();
        }
    }

    [PostgresFact]
    public async Task Revocation_during_the_provider_call_prevents_persisting_a_connection()
    {
        fixture.Client.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        fixture.Client.DuringExchange = async () =>
        {
            using var scope = fixture.Store.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().Users.IgnoreQueryFilters()
                .Where(User => User.Id == owner.UserId)
                .ExecuteUpdateAsync(Setters => Setters.SetProperty(User => User.IsActive, false));
        };

        using var response = await CallbackAsync(start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, fixture.Client.ExchangeCalls);
        using var verification = fixture.Store.Services.CreateScope();
        var db = verification.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.False(await db.ProviderConnections.IgnoreQueryFilters().AnyAsync(Connection => Connection.EnterpriseId == owner.EnterpriseId));
        Assert.NotNull((await db.Set<StoreCommerce.Domain.Entity.ProviderAuthorization>().IgnoreQueryFilters()
            .SingleAsync(Authorization => Authorization.EnterpriseId == owner.EnterpriseId)).ConsumedAt);
    }

    [PostgresFact]
    public async Task A_token_for_another_account_cannot_connect_the_store()
    {
        fixture.Client.Reset();
        fixture.Client.ReturnMismatchedAccount = true;
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        using var response = await CallbackAsync(start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var scope = fixture.Store.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().ProviderConnections.IgnoreQueryFilters()
            .AnyAsync(Connection => Connection.EnterpriseId == owner.EnterpriseId));
    }

    [PostgresFact]
    public async Task Signed_notification_delivery_is_deduplicated_and_invalid_signatures_are_rejected()
    {
        fixture.Client.Reset();
        var owner = await fixture.Store.CreateStoreAsync();
        var start = await BeginAsync(owner);
        using var completion = await CallbackAsync(start.State, start.Cookie);
        Assert.Equal(HttpStatusCode.Redirect, completion.StatusCode);
        using var scope = fixture.Store.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var connectionId = await db.ProviderConnections.IgnoreQueryFilters().Where(Connection => Connection.EnterpriseId == owner.EnterpriseId)
            .Select(Connection => Connection.Id).SingleAsync();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        const string paymentId = "123456";
        const string requestId = "test-delivery-1";
        var manifest = $"id:{paymentId};request-id:{requestId};ts:{timestamp};";
        var digest = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(MercadoPagoWorkflowFixture.SignatureSecret), Encoding.UTF8.GetBytes(manifest)));
        using var invalid = await NotifyAsync(connectionId, paymentId, requestId, $"ts={timestamp},v1={new string('0', 64)}");
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        using var first = await NotifyAsync(connectionId, paymentId, requestId, $"ts={timestamp},v1={digest}");
        using var repeated = await NotifyAsync(connectionId, paymentId, requestId, $"ts={timestamp},v1={digest}");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        Assert.Equal(1, await db.Set<StoreCommerce.Domain.Entity.ProviderNotification>().IgnoreQueryFilters()
            .CountAsync(Notification => Notification.EnterpriseId == owner.EnterpriseId));

        using var client = fixture.Store.Client(owner.Token);
        using var pending = await client.GetAsync("/api/integrations/mercado-pago/notifications");
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);
        using var json = JsonDocument.Parse(await pending.Content.ReadAsStringAsync());
        Assert.Single(json.RootElement.EnumerateArray());
    }

    private async Task<(string State, string Cookie)> BeginAsync(StoreActor owner)
    {
        using var client = fixture.Store.Client(owner.Token);
        using var response = await client.PostAsJsonAsync("/api/integrations/mercado-pago/oauth/start", new { environment = IntegrationEnvironment.Sandbox });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var url = new Uri(json.RootElement.GetProperty("authorizationUrl").GetString()!);
        var state = QueryHelpers.ParseQuery(url.Query)["state"].ToString();
        var cookie = response.Headers.GetValues("Set-Cookie").Single(Value => Value.StartsWith("__Host-mp-oauth=", StringComparison.Ordinal)).Split(';')[0];

        return (state, cookie);
    }

    private async Task<HttpResponseMessage> CallbackAsync(string state, string cookie)
    {
        using var client = fixture.Store.Client();
        using var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/integrations/mercado-pago/oauth/callback?state=" + Uri.EscapeDataString(state) + "&code=test-code");
        request.Headers.Add("Cookie", cookie);

        return await client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> NotifyAsync(long connectionId, string paymentId, string requestId, string signature)
    {
        using var client = fixture.Store.Client();
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/integrations/mercado-pago/webhooks/{connectionId}?data.id={paymentId}");
        request.Headers.Add("x-request-id", requestId);
        request.Headers.Add("x-signature", signature);
        request.Content = JsonContent.Create(new { type = "payment", data = new { id = paymentId } });

        return await client.SendAsync(request);
    }
}

public sealed class MercadoPagoWorkflowFixture : IAsyncLifetime
{
    public const string SignatureSecret = "test-only-mercado-pago-secret";
    public FakeMercadoPagoAuthorizationClient Client { get; } = new();
    public ContractsApiFixture Store { get; }

    public MercadoPagoWorkflowFixture()
    {
        Store = new ContractsApiFixture
        {
            ConfigureServices = Services =>
            {
                Services.AddSingleton<IMercadoPagoAuthorizationClient>(Client);
                Services.Configure<MercadoPagoOptions>(Options =>
                {
                    Options.Enabled = true;
                    Options.AllowProduction = false;
                    Options.ClientId = "test-client";
                    Options.ClientSecret = "test-client-secret";
                    Options.PublicApiBaseUrl = "https://api.example.invalid";
                    Options.BackofficeReturnUrl = "https://backoffice.example.invalid/integrations";
                    Options.PaymentReturnUrl = "https://shops.example.invalid/payment";
                    Options.SandboxWebhookSecret = SignatureSecret;
                });
                Services.Configure<ProviderIntegrationOptions>(Options => Options.KeyRingPath = "ephemeral-tests-only");
            }
        };
    }

    public Task InitializeAsync()
    {
        return Store.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return Store.DisposeAsync();
    }
}

public sealed class FakeMercadoPagoAuthorizationClient : IMercadoPagoAuthorizationClient
{
    private int exchangeCalls;
    public int ExchangeCalls => exchangeCalls;
    public bool ReturnMismatchedAccount { get; set; }
    public Func<Task>? DuringExchange { get; set; }

    public void Reset()
    {
        exchangeCalls = 0;
        ReturnMismatchedAccount = false;
        DuringExchange = null;
    }

    public string AuthorizationUrl(string state, string verifier)
    {
        return "https://provider.example.invalid/authorization?state=" + Uri.EscapeDataString(state);
    }

    public async Task<MercadoPagoAuthorizedAccount> ExchangeAsync(string code, string verifier, IntegrationEnvironment environment, DateTime now, CancellationToken ct)
    {
        Interlocked.Increment(ref exchangeCalls);
        Assert.Equal("test-code", code);
        Assert.Equal(IntegrationEnvironment.Sandbox, environment);
        Assert.False(string.IsNullOrWhiteSpace(verifier));

        if (DuringExchange is not null)
            await DuringExchange();

        return new MercadoPagoAuthorizedAccount("test-account", ReturnMismatchedAccount ? "another-account" : "test-account", "test-scope",
            new ProviderTokens("test-fake-access", null, now.AddHours(1), null));
    }

    public Task<string> SandboxAccountAsync(CancellationToken ct)
    {
        return Task.FromResult("test-account");
    }
}
