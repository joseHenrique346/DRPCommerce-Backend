using StoreCommerce.Application.Interfaces.Billing;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Features.Commands;
using DropCommerce.Application.Features.Commands;
using MediatR;
using DropCommerce.Application.Contracts;
using StoreCommerce.Application.Contracts;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DropCommerce.Api.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedCommerce.Api.Configuration;
using SharedCommerce.Application.Options;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Billing;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Payments;

namespace StoreCommerce.Contracts.Tests;

internal sealed class CommercialScenario : IAsyncDisposable
{
    public BillingClock Clock { get; } = new();
    public BillingProviderFake Billing { get; } = new();
    public ScenarioSalesGateway Sales { get; } = new();
    public ContractsApiFixture Store { get; }
    public StoreActor Owner { get; private set; } = null!;
    public DropAppearanceTestScope DropDatabase { get; } = new();
    public WebApplication Drop { get; private set; } = null!;

    public CommercialScenario()
    {
        Clock.Set(new DateTime(DateTime.UtcNow.Ticks / 10 * 10, DateTimeKind.Utc));
        Store = new ContractsApiFixture
        {
            ConfigureServices = Services =>
            {
                Services.Configure<PlatformBillingOptions>(Settings =>
                {
                    Settings.Enabled = true;
                    Settings.AccountId = "123";
                    Settings.AccessToken = "scenario-token-not-a-real-credential";
                    Settings.WebhookSecret = "scenario-webhook-not-a-real-secret";
                    Settings.PublicApiBaseUrl = "https://api.example.invalid";
                    Settings.ReturnUrl = "https://backoffice.example.invalid/billing";
                });
                Services.AddSingleton<IPlatformSubscriptionProvider>(Billing);
                Services.AddSingleton<IPlatformBillingProvider>(Billing);
                Services.AddSingleton<IDropSaleGateway>(Sales);
                Services.UseHandlerClock(Clock, typeof(StoreCommerce.Application.DependencyInjection).Assembly);
            }
        };
    }

    public async Task InitializeAsync()
    {
        await Store.InitializeAsync();
        Owner = await Store.CreateStoreAsync();
        await DropDatabase.InitializeAsync(Store, Owner);
        var security = Store.Services.GetRequiredService<IOptions<SecurityOptions>>().Value;
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = typeof(DropCatalogController).Assembly.GetName().Name
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = DropDatabase.DatabaseConnectionString,
            ["StoreApi:BaseUrl"] = "https://store.internal.invalid",
            ["Security:SigningKey"] = security.SigningKey,
            ["Security:Issuer"] = security.Issuer,
            ["Security:InternalServiceKey"] = security.InternalServiceKey,
            ["Security:BackofficeUrl"] = security.BackofficeUrl,
            ["Security:ApiRequestsPerMinute"] = "10000",
            ["DropQueue:PurchaseIdentityKey"] = "scenario-only-identity-key-not-for-production",
            ["DropQueue:ConcurrentBuyersPerEvent"] = "2"
        });
        var oldConnection = Environment.GetEnvironmentVariable("DROPCOMMERCE_CONNECTION_STRING");
        var oldStoreUrl = Environment.GetEnvironmentVariable("STORECOMMERCE_API_BASE_URL");

        try
        {
            Environment.SetEnvironmentVariable("DROPCOMMERCE_CONNECTION_STRING", DropDatabase.DatabaseConnectionString);
            Environment.SetEnvironmentVariable("STORECOMMERCE_API_BASE_URL", "https://store.internal.invalid");
            DropCommerce.Api.Configuration.DependencyInjectionConfig.AddApiConfiguration(builder.Services, builder.Configuration);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DROPCOMMERCE_CONNECTION_STRING", oldConnection);
            Environment.SetEnvironmentVariable("STORECOMMERCE_API_BASE_URL", oldStoreUrl);
        }

        builder.Services.AddControllers().AddApplicationPart(typeof(DropCatalogController).Assembly);
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Services.ConfigureAll<HttpClientFactoryOptions>(Options =>
            Options.HttpMessageHandlerBuilderActions.Add(Handler => Handler.PrimaryHandler = Store.CreateHandler()));
        builder.Services.UseHandlerClock(Clock, typeof(DropCommerce.Application.DependencyInjection).Assembly);

        foreach (var service in builder.Services.Where(Service => Service.ServiceType == typeof(IHostedService) &&
            Service.ImplementationType?.Namespace?.StartsWith("DropCommerce.", StringComparison.Ordinal) == true).ToArray())
            builder.Services.Remove(service);

        Drop = builder.Build();
        Drop.UseExceptionHandler();
        Drop.UseCommerceSecurity();
        Drop.MapControllers();
        await Drop.StartAsync();
    }

    public HttpClient Client(string? token = null, Uri? address = null)
    {
        var client = Drop.GetTestClient();
        client.BaseAddress = address ?? new Uri("https://backoffice.example.invalid");

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public async Task<PlatformBillingSummary> ChooseAsync(long planId)
    {
        using var scope = Store.Services.CreateScope();
        var contract = await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().Enterprises.IgnoreQueryFilters()
            .AsNoTracking().SingleAsync(Enterprise => Enterprise.Id == Owner.EnterpriseId);
        using var client = Store.Client(await Store.FreshTokenAsync(Owner));
        var change = await JsonAsync(await client.PostAsJsonAsync("/api/enterprises/current/contract/requests", new
        {
            idempotencyKey = Guid.NewGuid().ToString("N"), planId, catalogVersion = 1,
            expectedContractRevision = contract.ContractRevision, retainedMembershipIds = Array.Empty<long>()
        }));
        var checkout = await JsonAsync(await client.PostAsJsonAsync("/api/enterprises/current/billing/checkout", new
        {
            contractChangeId = change.GetProperty("id").GetInt64()
        }));

        return checkout.Deserialize<PlatformBillingSummary>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    public async Task<PlatformSubscription> SubscriptionAsync()
    {
        using var scope = Store.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().Set<PlatformSubscription>()
            .IgnoreQueryFilters().AsNoTracking().SingleAsync(Subscription => Subscription.EnterpriseId == Owner.EnterpriseId);
    }

    public async Task ApproveSubscriptionAsync()
    {
        var subscription = await SubscriptionAsync();
        var remote = Billing.Subscriptions[subscription.RemoteId!];
        Billing.Subscriptions[remote.Id] = remote with { Status = "authorized" };
        Billing.AddInvoice(remote.Id, Clock.GetUtcNow().UtcDateTime.AddSeconds(-1));
        await ReconcileBillingAsync();
    }

    public async Task ReconcileBillingAsync()
    {
        using var client = Store.Client(await Store.FreshTokenAsync(Owner));
        await JsonAsync(await client.PostAsync("/api/enterprises/current/billing/reconcile", null));
    }

    public static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        using (response)
        {
            var body = await response.Content.ReadAsStringAsync();
            Xunit.Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");

            return JsonDocument.Parse(body).RootElement.Clone();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Drop is not null)
        {
            await Drop.StopAsync();
            await Drop.DisposeAsync();
        }

        await DropDatabase.DisposeAsync();
        await Store.DisposeAsync();
    }
}

internal sealed class ScenarioSalesGateway : IDropSaleGateway
{
    public ConcurrentDictionary<string, PaymentRequest> Requests { get; } = new();
    public string Status { get; set; } = "pending";

    public Task<PaymentProviderContext> ContextAsync(long enterpriseId, CancellationToken ct)
    {
        return Task.FromResult(new PaymentProviderContext("mercadopago", IntegrationEnvironment.Sandbox, "test-seller-" + enterpriseId, null));
    }

    public Task<PaymentSession?> CreateAsync(PaymentRequest request, bool recoverOnly, CancellationToken ct)
    {
        if (!recoverOnly)
            Xunit.Assert.True(Requests.TryAdd(request.ExternalReference, request), "Uma correlação não cria outra cobrança.");

        return Task.FromResult<PaymentSession?>(Requests.ContainsKey(request.ExternalReference)
            ? new("scenario-preference-" + request.IdempotencyKey, "https://sandbox.mercadopago.com.br/scenario", null, request.ExpiresAt,
                "scenario-preference-" + request.IdempotencyKey)
            : null);
    }

    public Task<DropSaleGatewayResult> ReadAsync(PaymentRequest request, CancellationToken ct)
    {
        return Task.FromResult(new DropSaleGatewayResult(Status, Status == "approved" ? "scenario-payment-" + request.IdempotencyKey : null));
    }
}
