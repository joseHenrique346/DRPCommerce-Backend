using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

internal sealed class MelhorEnvioOAuthHttpStub : HttpMessageHandler
{
    private int exchangeCalls;
    private int refreshCalls;
    public int ExchangeCalls => exchangeCalls;
    public int RefreshCalls => refreshCalls;
    public Func<Task>? DuringExchange { get; set; }

    public void Reset()
    {
        exchangeCalls = 0;
        refreshCalls = 0;
        DuringExchange = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Assert.Equal("https", request.RequestUri!.Scheme);
        Assert.Equal("sandbox.melhorenvio.com.br", request.RequestUri.Host);
        Assert.Equal("DRPCommerce (integrations@example.invalid)", request.Headers.UserAgent.ToString());
        Assert.Contains(request.Headers.Accept, MediaType => MediaType.MediaType == "application/json");

        if (request.RequestUri.AbsolutePath == "/api/v2/me")
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Contains(request.Headers.Authorization?.Parameter, new[] { "test-only-access", "test-only-access-renewed" });

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { id = 314 }) };
        }

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/oauth/token", request.RequestUri.AbsolutePath);
        Assert.Equal("application/x-www-form-urlencoded", request.Content!.Headers.ContentType?.MediaType);
        var payload = QueryHelpers.ParseQuery(await request.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal("123", payload["client_id"].ToString());
        Assert.Equal("test-only-client-secret", payload["client_secret"].ToString());

        if (payload["grant_type"].ToString() == "refresh_token")
        {
            Interlocked.Increment(ref refreshCalls);
            Assert.Equal("test-only-refresh", payload["refresh_token"].ToString());

            return Tokens("test-only-access-renewed", "test-only-refresh-renewed", 2592000);
        }

        Interlocked.Increment(ref exchangeCalls);
        Assert.Equal("authorization_code", payload["grant_type"].ToString());
        Assert.Equal("test-code", payload["code"].ToString());
        Assert.Equal("https://api.example.invalid/api/integrations/melhor-envio/oauth/sandbox/callback",
            payload["redirect_uri"].ToString());

        if (DuringExchange is not null)
            await DuringExchange();

        return Tokens("test-only-access", "test-only-refresh", 45);
    }

    private static HttpResponseMessage Tokens(string accessToken, string refreshToken, int expiresIn)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                token_type = "Bearer",
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_in = expiresIn
            })
        };
    }
}
