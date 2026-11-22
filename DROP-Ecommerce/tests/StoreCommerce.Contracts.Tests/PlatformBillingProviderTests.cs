using StoreCommerce.Domain.Enums.Integrations;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Billing;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Billing;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class PlatformBillingProviderTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
    private const string Reference = "drp-saas-0123456789abcdef0123456789abcdef";

    [Fact]
    public async Task Checkout_uses_only_platform_credential_and_validated_server_terms()
    {
        using var scope = new ProviderScope();
        var checkout = await scope.Provider.CreateAsync(Charge(), default);

        Assert.Equal("preference-1", checkout.PreferenceId);
        Assert.Equal("https://www.mercadopago.com.br/checkout/1", checkout.CheckoutUrl);
        Assert.Equal(1, scope.Handler.Posts);
        Assert.All(scope.Handler.Tokens, Token => Assert.Equal("private-platform-test-token", Token));
        using var body = JsonDocument.Parse(scope.Handler.LastBody!);
        var root = body.RootElement;
        Assert.Equal(Reference, root.GetProperty("external_reference").GetString());
        Assert.Equal(58.90m, root.GetProperty("items")[0].GetProperty("unit_price").GetDecimal());
        Assert.Equal("https://api.example.invalid/api/platform-billing/mercado-pago/webhook",
            root.GetProperty("notification_url").GetString());
        Assert.Contains("billingReference=" + Reference, root.GetProperty("back_urls").GetProperty("success").GetString());
        Assert.False(root.TryGetProperty("marketplace_fee", out _));
        Assert.False(root.TryGetProperty("payer", out _));
    }

    [Theory]
    [InlineData(false, IntegrationEnvironment.Sandbox, false)]
    [InlineData(true, IntegrationEnvironment.Production, false)]
    [InlineData(true, (IntegrationEnvironment)99, true)]
    public async Task Disabled_or_unapproved_environment_never_calls_provider(bool enabled,
        IntegrationEnvironment environment, bool productionAllowed)
    {
        using var scope = new ProviderScope();
        scope.Settings.Enabled = enabled;
        scope.Settings.Environment = environment;
        scope.Settings.AllowProduction = productionAllowed;

        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.CreateAsync(Charge(), default));

        Assert.Equal("Billing.Disabled", failure.Code);
        Assert.Empty(scope.Handler.Tokens);
    }

    [Theory]
    [InlineData("999", true, "MLB", "Billing.AccountMismatch")]
    [InlineData("123", false, "MLB", "MercadoPago.EnvironmentMismatch")]
    [InlineData("123", true, "MLA", "MercadoPago.EnvironmentMismatch")]
    public async Task Wrong_account_or_test_identity_stops_before_creating_charge(string account,
        bool testUser, string site, string code)
    {
        using var scope = new ProviderScope();
        scope.Handler.AccountId = account;
        scope.Handler.TestUser = testUser;
        scope.Handler.Site = site;

        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.CreateAsync(Charge(), default));

        Assert.Equal(code, failure.Code);
        Assert.Equal(0, scope.Handler.Posts);
    }

    [Theory]
    [InlineData("account")]
    [InlineData("environment")]
    [InlineData("amount")]
    [InlineData("precision")]
    [InlineData("currency")]
    [InlineData("expired")]
    [InlineData("reference")]
    public async Task Invalid_or_rotated_snapshot_never_calls_provider(string field)
    {
        using var scope = new ProviderScope();
        var charge = field switch
        {
            "account" => Charge() with { AccountId = "999" },
            "environment" => Charge() with { Environment = IntegrationEnvironment.Production },
            "amount" => Charge() with { Amount = 0 },
            "precision" => Charge() with { Amount = 1.001m },
            "currency" => Charge() with { Currency = "USD" },
            "expired" => Charge() with { CheckoutExpiresAt = Now.UtcDateTime },
            _ => Charge() with { ExternalReference = "shop-order-123" }
        };

        await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.CreateAsync(charge, default));

        Assert.Empty(scope.Handler.Tokens);
    }

    [Theory]
    [InlineData("http://www.mercadopago.com.br/checkout")]
    [InlineData("https://www.mercadopago.com.br.evil.invalid/checkout")]
    [InlineData("https://user@www.mercadopago.com.br/checkout")]
    [InlineData("https://www.mercadopago.com.br:8443/checkout")]
    public async Task Checkout_rejects_untrusted_redirect(string url)
    {
        using var scope = new ProviderScope();
        scope.Handler.CheckoutUrl = url;

        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.CreateAsync(Charge(), default));

        Assert.Equal("Billing.InvalidResponse", failure.Code);
        Assert.Equal(1, scope.Handler.Posts);
    }

    [Theory]
    [InlineData("receiver")]
    [InlineData("reference")]
    [InlineData("amount")]
    [InlineData("currency")]
    public async Task Checkout_rejects_terms_changed_by_provider(string field)
    {
        using var scope = new ProviderScope();
        scope.Handler.Mismatch = field;

        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.CreateAsync(Charge(), default));

        Assert.Equal("Billing.ScopeMismatch", failure.Code);
    }

    [Fact]
    public async Task Lost_response_is_recovered_by_reference_without_another_post_even_after_expiry()
    {
        using var scope = new ProviderScope();
        scope.Handler.LoseResponse = true;
        await Assert.ThrowsAsync<HttpRequestException>(() => scope.Provider.CreateAsync(Charge(), default));

        var recovered = await scope.Provider.RecoverAsync(Charge() with
        {
            CheckoutExpiresAt = Now.AddMinutes(-1).UtcDateTime
        }, default);

        Assert.Equal("preference-1", recovered!.PreferenceId);
        Assert.Equal(1, scope.Handler.Posts);
        Assert.Contains(scope.Handler.Paths, Path => Path.Contains("external_reference=" + Reference));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task Empty_or_ambiguous_recovery_never_posts_again(int total)
    {
        using var scope = new ProviderScope();
        scope.Handler.SearchTotal = total;

        if (total == 0)
        {
            Assert.Null(await scope.Provider.RecoverAsync(Charge(), default));
        }
        else
        {
            var failure = await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.RecoverAsync(Charge(), default));
            Assert.Equal("Billing.AmbiguousPreference", failure.Code);
        }

        Assert.Equal(0, scope.Handler.Posts);
    }

    [Fact]
    public async Task Sandbox_test_seller_can_return_live_mode_true_without_becoming_real_account()
    {
        using var scope = new ProviderScope();
        var payment = await scope.Provider.GetPaymentAsync("456", default);

        Assert.Equal("approved", payment.Status);
        Assert.Equal("123", payment.AccountId);
        Assert.Equal(58.90m, payment.Amount);
        Assert.Equal(Reference, payment.ExternalReference);
        Assert.Equal(0, scope.Handler.Posts);
    }

    [Theory]
    [InlineData("receiver")]
    [InlineData("id")]
    [InlineData("date")]
    public async Task Payment_requires_expected_receiver_identifier_and_update_date(string field)
    {
        using var scope = new ProviderScope();
        scope.Handler.Mismatch = field;

        await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.GetPaymentAsync("456", default));

        Assert.Equal(0, scope.Handler.Posts);
    }

    [Fact]
    public async Task Real_account_is_required_for_explicitly_enabled_production()
    {
        using var scope = new ProviderScope();
        scope.Settings.Environment = IntegrationEnvironment.Production;
        scope.Settings.AllowProduction = true;

        await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.GetPaymentAsync("456", default));

        scope.Handler.TestUser = false;
        scope.Handler.Live = false;
        await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.GetPaymentAsync("456", default));

        scope.Handler.Live = true;
        Assert.Equal("approved", (await scope.Provider.GetPaymentAsync("456", default)).Status);
    }

    [Theory]
    [InlineData("reference")]
    [InlineData("truncated")]
    public async Task Payment_search_rejects_other_reference_or_incomplete_results(string field)
    {
        using var scope = new ProviderScope();
        scope.Handler.Mismatch = field;

        await Assert.ThrowsAsync<SecurityFailure>(() => scope.Provider.SearchPaymentsAsync(Reference, default));
    }

    [Fact]
    public void Platform_and_store_webhooks_require_different_secrets_and_keep_existing_manifest_format()
    {
        using var scope = new ProviderScope();
        var platform = new PlatformBillingSignature(Options.Create(scope.Settings), new FixedTime());
        var store = new MercadoPagoSignature(Options.Create(new MercadoPagoOptions
        {
            SandboxWebhookSecret = "store-secret"
        }), new FixedTime());
        var signature = Sign(scope.Settings.WebhookSecret, Now);
        var delivery = platform.Verify("456", "request-1", signature);

        Assert.Equal(64, delivery.Length);
        Assert.Throws<SecurityFailure>(() => store.Verify("456", "request-1", signature));
        Assert.Equal(delivery, store.Verify("456", "request-1", Sign("store-secret", Now)));
        Assert.Throws<SecurityFailure>(() => platform.Verify("456", "request-1", Sign(scope.Settings.WebhookSecret, Now.AddHours(-1))));
        Assert.Throws<SecurityFailure>(() => platform.Verify("457", "request-1", signature));
        Assert.Throws<SecurityFailure>(() => platform.Verify("456", "request-1", signature + ",ts=1"));
    }

    private static string Sign(string secret, DateTimeOffset at)
    {
        var timestamp = at.ToUnixTimeSeconds();
        var manifest = $"id:456;request-id:request-1;ts:{timestamp};";
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(manifest));

        return $"ts={timestamp},v1={Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private static PlatformBillingCharge Charge()
    {
        return new PlatformBillingCharge(Reference, "123", IntegrationEnvironment.Sandbox, 58.90m, "BRL", Now.AddMinutes(30).UtcDateTime);
    }

    private sealed class FixedTime : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return Now;
        }
    }

    private sealed class ProviderScope : IDisposable
    {
        public PlatformBillingOptions Settings { get; } = new()
        {
            Enabled = true,
            AccountId = "123",
            AccessToken = "private-platform-test-token",
            WebhookSecret = "platform-secret",
            PublicApiBaseUrl = "https://api.example.invalid",
            ReturnUrl = "https://backoffice.example.invalid/billing"
        };
        public ProviderHandler Handler { get; } = new();
        public MercadoPagoPlatformBillingProvider Provider { get; }
        private readonly HttpClient client;

        public ProviderScope()
        {
            client = new HttpClient(Handler) { BaseAddress = new Uri("https://api.mercadopago.com") };
            Provider = new MercadoPagoPlatformBillingProvider(new MercadoPagoApi(new ClientFactory(client)),
                Options.Create(Settings), new FixedTime());
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    private sealed class ClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal("MercadoPago", name);

            return client;
        }
    }

    private sealed class ProviderHandler : HttpMessageHandler
    {
        public int Posts { get; private set; }
        public string? LastBody { get; private set; }
        public List<string?> Tokens { get; } = [];
        public List<string> Paths { get; } = [];
        public string AccountId { get; set; } = "123";
        public bool TestUser { get; set; } = true;
        public string Site { get; set; } = "MLB";
        public bool Live { get; set; } = true;
        public bool LoseResponse { get; set; }
        public int SearchTotal { get; set; } = 1;
        public string Mismatch { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = "https://www.mercadopago.com.br/checkout/1";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("api.mercadopago.com", request.RequestUri!.Host);
            Tokens.Add(request.Headers.Authorization?.Parameter);
            Paths.Add(request.RequestUri.PathAndQuery);
            object body;

            if (request.RequestUri.AbsolutePath == "/users/me")
            {
                body = new { id = AccountId, site_id = Site, tags = TestUser ? new[] { "test_user" } : [] };
            }
            else if (request.Method == HttpMethod.Post)
            {
                Posts++;
                LastBody = await request.Content!.ReadAsStringAsync(ct);

                if (LoseResponse)
                    throw new HttpRequestException("Simulated response loss.");

                body = Preference();
            }
            else if (request.RequestUri.AbsolutePath == "/checkout/preferences/search")
            {
                body = new
                {
                    total = SearchTotal,
                    elements = Enumerable.Range(0, SearchTotal).Select(Index => new { id = "preference-1" }).ToArray()
                };
            }
            else if (request.RequestUri.AbsolutePath.StartsWith("/checkout/preferences/"))
            {
                body = Preference();
            }
            else if (request.RequestUri.AbsolutePath == "/v1/payments/search")
            {
                body = new { paging = new { total = Mismatch == "truncated" ? 101 : 1 }, results = new[] { Payment() } };
            }
            else if (request.RequestUri.AbsolutePath == "/v1/payments/456")
            {
                body = Payment();
            }
            else
            {
                throw new InvalidOperationException("Unexpected provider path.");
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
        }

        private object Preference()
        {
            return new
            {
                id = "preference-1",
                collector_id = Mismatch == "receiver" ? "999" : "123",
                external_reference = Mismatch == "reference" ? "other-reference" : Reference,
                items = new[]
                {
                    new
                    {
                        quantity = 1,
                        currency_id = Mismatch == "currency" ? "USD" : "BRL",
                        unit_price = Mismatch == "amount" ? 1m : 58.90m
                    }
                },
                init_point = CheckoutUrl
            };
        }

        private object Payment()
        {
            return new
            {
                id = Mismatch == "id" ? "457" : "456",
                collector_id = Mismatch == "receiver" ? "999" : "123",
                external_reference = Mismatch == "reference" ? "other-reference" : Reference,
                transaction_amount = 58.90m,
                currency_id = "BRL",
                status = "approved",
                transaction_amount_refunded = 0,
                date_last_updated = Mismatch == "date" ? "invalid" : Now.ToString("O"),
                live_mode = Live
            };
        }
    }
}
