using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Infrastructure.Data.Billing;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class MercadoPagoSubscriptionsProviderTests
{
    private const string RemoteId = "0123456789abcdef0123456789abcdef";
    private const string Reference = "drp-saas-0123456789abcdef0123456789abcdef";

    [Fact]
    public async Task Hosted_monthly_authorization_uses_pending_without_receiving_card_data_or_creating_payment()
    {
        using var test = new ProviderTest();
        var subscription = await test.Provider.CreateAsync(Reference, "payer@example.invalid", 49, default);
        using var body = JsonDocument.Parse(test.Handler.Body!);
        var root = body.RootElement;
        Assert.Equal("pending", root.GetProperty("status").GetString());
        Assert.Equal("payer@example.invalid", root.GetProperty("payer_email").GetString());
        Assert.Equal(1, root.GetProperty("auto_recurring").GetProperty("frequency").GetInt32());
        Assert.Equal("months", root.GetProperty("auto_recurring").GetProperty("frequency_type").GetString());
        Assert.Equal(49, subscription.MonthlyAmount);
        Assert.False(root.TryGetProperty("card_token_id", out _));
        Assert.False(root.GetProperty("auto_recurring").TryGetProperty("free_trial", out _));
        Assert.Equal(new[] { "/preapproval" }, test.Handler.Writes);
    }

    [Fact]
    public async Task Amount_update_does_not_change_frequency_or_dates_and_cancel_uses_documented_status()
    {
        using var test = new ProviderTest();
        await test.Provider.UpdateAsync(RemoteId, 99, false, default);
        using (var body = JsonDocument.Parse(test.Handler.Body!))
        {
            var recurring = body.RootElement.GetProperty("auto_recurring");
            Assert.Equal(99, recurring.GetProperty("transaction_amount").GetDecimal());
            Assert.False(recurring.TryGetProperty("start_date", out _));
            Assert.False(recurring.TryGetProperty("frequency", out _));
        }

        await test.Provider.UpdateAsync(RemoteId, 0, true, default);
        using var canceled = JsonDocument.Parse(test.Handler.Body!);
        Assert.Equal("canceled", canceled.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Recurring_invoices_accept_documented_decimal_strings_and_follow_pagination()
    {
        using var test = new ProviderTest();
        test.Handler.Paginate = true;
        var invoices = await test.Provider.InvoicesAsync(RemoteId, default);
        Assert.Equal(101, invoices.Count);
        Assert.All(invoices, Invoice => Assert.Equal(49m, Invoice.Amount));
        Assert.All(invoices, Invoice => Assert.Equal(DateTimeKind.Utc, Invoice.DebitDate.Kind));
        Assert.Contains(test.Handler.Paths, Path => Path.Contains("offset=100"));
        Assert.Empty(test.Handler.Writes);
    }

    [Theory]
    [InlineData("collector")]
    [InlineData("frequency")]
    [InlineData("currency")]
    [InlineData("url")]
    [InlineData("reference")]
    public async Task Creation_rejects_foreign_or_modified_subscription_terms(string mismatch)
    {
        using var test = new ProviderTest();
        test.Handler.Mismatch = mismatch;
        await Assert.ThrowsAsync<SecurityFailure>(() => test.Provider.CreateAsync(Reference, "payer@example.invalid", 49, default));
    }

    [Fact]
    public async Task Recovery_and_invoice_search_cannot_accept_ambiguous_or_foreign_results()
    {
        using var test = new ProviderTest();
        Assert.NotNull(await test.Provider.RecoverAsync(Reference, default));
        test.Handler.Mismatch = "ambiguous";
        await Assert.ThrowsAsync<SecurityFailure>(() => test.Provider.RecoverAsync(Reference, default));
        test.Handler.Mismatch = "invoice-subscription";
        await Assert.ThrowsAsync<SecurityFailure>(() => test.Provider.InvoicesAsync(RemoteId, default));
        Assert.Empty(test.Handler.Writes);
    }

    private sealed class ProviderTest : IDisposable
    {
        public SubscriptionHandler Handler { get; } = new();
        private readonly HttpClient client;
        public MercadoPagoSubscriptionProvider Provider { get; }

        public ProviderTest()
        {
            client = new HttpClient(Handler) { BaseAddress = new Uri("https://api.mercadopago.com") };
            Provider = new MercadoPagoSubscriptionProvider(new MercadoPagoApi(new ClientFactory(client)),
                Options.Create(new PlatformBillingOptions
                {
                    Enabled = true, AccountId = "123", AccessToken = "only-platform-token", WebhookSecret = "platform-signature",
                    PublicApiBaseUrl = "https://api.example.invalid", ReturnUrl = "https://backoffice.example.invalid/billing"
                }));
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
            return client;
        }
    }

    private sealed class SubscriptionHandler : HttpMessageHandler
    {
        public string? Body { get; private set; }
        public List<string> Writes { get; } = [];
        public List<string> Paths { get; } = [];
        public string Mismatch { get; set; } = string.Empty;
        public bool Paginate { get; set; }
        private decimal monthlyAmount = 49;
        private string status = "pending";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("only-platform-token", request.Headers.Authorization!.Parameter);
            Paths.Add(request.RequestUri!.PathAndQuery);
            var path = request.RequestUri.AbsolutePath;
            object result;

            if (request.Method != HttpMethod.Get)
            {
                Writes.Add(path);
                Body = await request.Content!.ReadAsStringAsync(ct);
                using var json = JsonDocument.Parse(Body);

                if (json.RootElement.TryGetProperty("auto_recurring", out var auto))
                    monthlyAmount = auto.GetProperty("transaction_amount").GetDecimal();

                if (json.RootElement.TryGetProperty("status", out var state))
                    status = state.GetString()!;

                result = Subscription();
            }
            else if (path == "/users/me")
            {
                result = new { id = 123, site_id = "MLB", tags = new[] { "test_user" } };
            }
            else if (path == "/preapproval/search")
            {
                var count = Mismatch == "ambiguous" ? 2 : 1;
                result = new { paging = new { total = count }, results = Enumerable.Range(0, count).Select(Index => new { id = RemoteId }).ToArray() };
            }
            else if (path == "/authorized_payments/search")
            {
                var offset = request.RequestUri.Query.Contains("offset=100") ? 100 : 0;
                var count = Paginate && offset == 0 ? 100 : 1;
                result = new
                {
                    paging = new { total = Paginate ? 101 : 1 },
                    results = Enumerable.Range(offset, count).Select(Index => new
                    {
                        id = Index + 1000,
                        preapproval_id = Mismatch == "invoice-subscription" ? "foreign" : RemoteId,
                        transaction_amount = "49.00", currency_id = "BRL", debit_date = "2026-11-20T12:00:00Z",
                        payment = new { id = Index + 2000, status = "approved" }
                    }).ToArray()
                };
            }
            else
            {
                result = Subscription();
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json")
            };
        }

        private object Subscription()
        {
            return new
            {
                id = RemoteId, collector_id = Mismatch == "collector" ? "999" : "123",
                external_reference = Mismatch == "reference" ? "foreign" : Reference, status,
                init_point = Mismatch == "url" ? "https://evil.invalid/checkout" : "https://www.mercadopago.com.br/subscriptions/checkout",
                auto_recurring = new
                {
                    frequency = Mismatch == "frequency" ? 12 : 1, frequency_type = "months",
                    currency_id = Mismatch == "currency" ? "USD" : "BRL", transaction_amount = monthlyAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }
            };
        }
    }
}
