using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class StorefrontPathContractsTests(ContractsApiFixture fixture)
{
    [PostgresFact]
    public async Task Same_host_keeps_refresh_and_bearer_sessions_separate_by_storefront_path()
    {
        var first = await fixture.CreateStoreAsync();
        var second = await fixture.CreateStoreAsync();
        var firstStorefront = await fixture.CreateCustomerAsync(first.EnterpriseId);
        var secondStorefront = await fixture.CreateCustomerAsync(second.EnterpriseId);
        var credentials = new { email = "buyer-" + Guid.NewGuid().ToString("N") + "@example.invalid", password = "P4ssword-test-storefront!" };
        using var browser = fixture.Client();
        var csrf = await CsrfAsync(browser, firstStorefront.Address);

        var signup = new
        {
            fullName = "Comprador",
            credentials.email,
            credentials.password,
            phone = "11999999999",
            addressLine = "Rua Teste",
            city = "São Paulo",
            stateId = 1,
            zipCode = "01001000",
            country = "BR",
            gender = "",
            dateOfBirth = "2000-01-01T00:00:00Z"
        };
        using var firstRegister = await AuthAsync(browser, firstStorefront.Address, "register", csrf, JsonContent.Create(signup));
        firstRegister.EnsureSuccessStatusCode();
        using var secondRegister = await AuthAsync(browser, secondStorefront.Address, "register", csrf, JsonContent.Create(signup));
        secondRegister.EnsureSuccessStatusCode();
        using var firstLogin = await AuthAsync(browser, firstStorefront.Address, "login", csrf, JsonContent.Create(credentials));
        using var secondLogin = await AuthAsync(browser, secondStorefront.Address, "login", csrf, JsonContent.Create(credentials));
        firstLogin.EnsureSuccessStatusCode();
        secondLogin.EnsureSuccessStatusCode();
        var firstCookie = Cookie(firstLogin, $"__Host-DRP-StorefrontRefresh-{first.EnterpriseId}=");
        var secondCookie = Cookie(secondLogin, $"__Host-DRP-StorefrontRefresh-{second.EnterpriseId}=");
        using var firstSession = JsonDocument.Parse(await firstLogin.Content.ReadAsStringAsync());
        using var bearer = fixture.Client(firstSession.RootElement.GetProperty("accessToken").GetString());
        using var authorized = await bearer.GetAsync(new Uri(firstStorefront.Address, "cart"));
        using var unauthorized = await bearer.GetAsync(new Uri(secondStorefront.Address, "cart"));
        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var forgedCookie = secondCookie[..secondCookie.IndexOf('=')] + firstCookie[firstCookie.IndexOf('=')..];
        using var rejectedRefresh = await AuthAsync(browser, secondStorefront.Address, "refresh", csrf, refresh: forgedCookie);
        Assert.Equal(HttpStatusCode.Unauthorized, rejectedRefresh.StatusCode);
        var cookies = firstCookie + "; " + secondCookie;
        using var secondRefresh = await AuthAsync(browser, secondStorefront.Address, "refresh", csrf, refresh: cookies);
        secondRefresh.EnsureSuccessStatusCode();
        Assert.StartsWith($"__Host-DRP-StorefrontRefresh-{second.EnterpriseId}=", Cookie(secondRefresh, "__Host-DRP-StorefrontRefresh-"));
        using var firstRefresh = await AuthAsync(browser, firstStorefront.Address, "refresh", csrf, refresh: cookies);
        firstRefresh.EnsureSuccessStatusCode();

        using var oldPath = await browser.GetAsync(new Uri(firstStorefront.Address, "/api/storefront/cart"));
        Assert.Equal(HttpStatusCode.NotFound, oldPath.StatusCode);
    }

    [PostgresFact]
    public async Task Registration_creates_unique_storefront_path_without_domain_and_returns_public_url()
    {
        var admin = await fixture.CreateUserAsync();
        var slug = "loja-" + Guid.NewGuid().ToString("N");
        using var client = fixture.Client(admin.Token);
        var registration = new
        {
            tradeName = "Loja por caminho",
            legalName = "Loja por caminho Ltda.",
            email = admin.Email,
            phone = "11999999999",
            addressLine = "Rua Teste",
            city = "São Paulo",
            stateId = 1,
            zipCode = "01001000",
            country = "BR",
            slug
        };
        using var registered = await client.PostAsJsonAsync("/api/enterprises/register", registration);
        registered.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await registered.Content.ReadAsStringAsync());
        Assert.Equal(slug, json.RootElement.GetProperty("storefrontSlug").GetString());
        Assert.Equal("https://shops.example.invalid/" + slug, json.RootElement.GetProperty("storefrontUrl").GetString());
        using var duplicate = await client.PostAsJsonAsync("/api/enterprises/register", registration with { slug = slug.ToUpperInvariant() });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        using var storefront = fixture.Client();
        using var info = await storefront.GetAsync($"https://shops.example.invalid/api/storefront/{slug}/store");
        info.EnsureSuccessStatusCode();
        using var publicStore = JsonDocument.Parse(await info.Content.ReadAsStringAsync());
        var content = publicStore.RootElement.GetProperty("content");
        Assert.Equal("Loja por caminho", content.GetProperty("tradeName").GetString());
        Assert.Equal(slug, content.GetProperty("storefrontSlug").GetString());
    }

    [PostgresFact]
    public async Task Migration_assigns_unique_paths_to_existing_stores_by_id()
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var slugs = await db.Enterprises.IgnoreQueryFilters()
            .ToDictionaryAsync(Enterprise => Enterprise.Id, Enterprise => Enterprise.StorefrontSlug);

        Assert.All(fixture.LegacyEnterpriseIds, Id => Assert.Equal($"loja-{Id}", slugs[Id]));
    }

    private static async Task<(string Cookie, string Token)> CsrfAsync(HttpClient browser, Uri storefront)
    {
        using var response = await browser.GetAsync(new Uri(storefront, "auth/csrf"));
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return (Cookie(response, "__Host-Commerce-Csrf="), json.RootElement.GetProperty("requestToken").GetString()!);
    }

    private static async Task<HttpResponseMessage> AuthAsync(HttpClient browser, Uri storefront, string action,
        (string Cookie, string Token) csrf, HttpContent? body = null, string? refresh = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(storefront, "auth/" + action)) { Content = body };
        request.Headers.Add("Cookie", refresh is null ? csrf.Cookie : csrf.Cookie + "; " + refresh);
        request.Headers.Add("X-CSRF-TOKEN", csrf.Token);

        return await browser.SendAsync(request);
    }

    private static string Cookie(HttpResponseMessage response, string prefix)
    {
        return response.Headers.GetValues("Set-Cookie").Single(Value => Value.StartsWith(prefix, StringComparison.Ordinal)).Split(';')[0];
    }
}
