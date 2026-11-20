using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Billing;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Billing;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[CollectionDefinition("platform-billing-postgres", DisableParallelization = true)]
public sealed class PlatformBillingCollection : ICollectionFixture<PlatformBillingFixture>
{
}

public sealed class PlatformBillingFixture : IAsyncLifetime
{
    public BillingClock Clock { get; } = new();
    public BillingProviderFake Provider { get; } = new();
    public ContractsApiFixture Api { get; }

    public PlatformBillingFixture()
    {
        Api = new ContractsApiFixture
        {
            ConfigureServices = services =>
            {
                services.Configure<PlatformBillingOptions>(Settings =>
                {
                    Settings.Enabled = true;
                    Settings.AccountId = "123";
                    Settings.AccessToken = "fake-private-token";
                    Settings.WebhookSecret = "fake-billing-secret";
                    Settings.PublicApiBaseUrl = "https://api.example.invalid";
                    Settings.ReturnUrl = "https://backoffice.example.invalid/billing";
                });
                services.AddSingleton<IPlatformSubscriptionProvider>(Provider);
                services.AddSingleton<IPlatformBillingProvider>(Provider);
                services.AddScoped(Service => ActivatorUtilities.CreateInstance<PlatformBillingService>(Service, Clock));
            }
        };
    }

    public Task InitializeAsync()
    {
        return Api.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return Api.DisposeAsync();
    }
}

[Collection("platform-billing-postgres")]
public sealed class PlatformSubscriptionsApiTests(PlatformBillingFixture fixture)
{
    private const string BillingPath = "/api/enterprises/current/billing";
    private ContractsApiFixture Api => fixture.Api;
    private BillingProviderFake Provider => fixture.Provider;
    private BillingClock Clock => fixture.Clock;

    [PostgresFact]
    public async Task Authorization_alone_does_not_grant_and_first_approved_invoice_grants_once()
    {
        Reset();
        var owner = await Api.CreateStoreAsync();
        var offerId = await RequestAsync(owner, 4);
        var subscription = await StartAsync(owner, offerId);
        Assert.Equal("awaiting_payment", subscription.Status);
        Assert.NotNull(subscription.CheckoutUrl);
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);
        var remote = await RemoteAsync(subscription.SubscriptionId!.Value);
        Provider.Subscriptions[remote.Id] = remote with { Status = "authorized" };
        await ReconcileAsync(owner, subscription.SubscriptionId.Value);
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);

        Provider.AddInvoice(remote.Id, Clock.GetUtcNow().UtcDateTime.AddSeconds(-1));
        await ReconcileAsync(owner, subscription.SubscriptionId.Value);
        var contract = await ContractAsync(owner);
        Assert.Equal(4, contract.PlanId);
        Assert.Equal(2, contract.ContractRevision);
        await ReconcileAsync(owner, subscription.SubscriptionId.Value);
        Assert.Equal(contract.ContractRevision, (await ContractAsync(owner)).ContractRevision);

        using var client = Api.Client(await Api.FreshTokenAsync(owner));
        var summary = await client.GetFromJsonAsync<PlatformBillingSummary>(BillingPath);
        Assert.Equal("active", summary!.Status);
        Assert.Equal(49m, summary.MonthlyAmount);
        using var response = await client.GetAsync(BillingPath + "/invoices");
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("fake-private-token", await response.Content.ReadAsStringAsync());
    }

    [PostgresFact]
    public async Task Upgrade_collects_only_proportional_difference_then_keeps_due_date_and_updates_automatic_renewal()
    {
        Reset();
        var (owner, id, remote) = await ActiveAsync(4);
        var before = await SubscriptionAsync(id);
        Clock.Set(before.PeriodStart!.Value.AddDays(10));
        var change = await RequestAsync(owner, 2);
        await StartAsync(owner, change);
        Assert.Equal(4, (await ContractAsync(owner)).PlanId);
        var invoice = await UpgradeInvoiceAsync(id);
        Assert.Equal(PlatformBillingPolicy.UpgradeAmount(49m, 99m, before.PeriodStart.Value, before.PaidUntil!.Value,
            invoice.PeriodStart), invoice.Amount);
        Assert.Equal(1, Provider.ChargePosts);
        Provider.PayUpgrade(invoice.Reference, invoice.Amount, Clock.GetUtcNow().UtcDateTime);
        await ReconcileAsync(owner, id);
        Assert.Equal(2, (await ContractAsync(owner)).PlanId);
        Assert.Equal(before.PaidUntil, (await SubscriptionAsync(id)).PaidUntil);
        Assert.Equal(99m, Provider.Subscriptions[remote.Id].MonthlyAmount);
        Assert.Equal(3, (await ContractAsync(owner)).ContractRevision);

        await ReconcileAsync(owner, id);
        Assert.Equal(3, (await ContractAsync(owner)).ContractRevision);
        Clock.Set(before.PaidUntil.Value);
        Provider.AddInvoice(remote.Id, before.PaidUntil.Value);
        await ReconcileAsync(owner, id);
        Assert.True((await SubscriptionAsync(id)).PaidUntil > before.PaidUntil);
        Assert.Equal(2, (await ContractAsync(owner)).PlanId);
    }

    [PostgresFact]
    public async Task Paid_downgrade_waits_for_boundary_and_owner_can_update_team_selection()
    {
        Reset();
        var (owner, id, remote) = await ActiveAsync(2);
        var staffA = await Api.CreateStaffAsync(owner.EnterpriseId);
        var staffB = await Api.CreateStaffAsync(owner.EnterpriseId);
        var until = (await SubscriptionAsync(id)).PaidUntil!.Value;
        var change = await RequestAsync(owner, 4, [staffA.MembershipId]);
        await StartAsync(owner, change);
        Assert.Equal(2, (await ContractAsync(owner)).PlanId);
        Assert.Equal(49m, Provider.Subscriptions[remote.Id].MonthlyAmount);

        using (var client = Api.Client(await Api.FreshTokenAsync(owner)))
        {
            using var response = await client.PutAsJsonAsync(BillingPath + "/pending-team", new
            {
                expectedContractRevision = (await ContractAsync(owner)).ContractRevision,
                retainedMembershipIds = new[] { staffB.MembershipId }
            });
            response.EnsureSuccessStatusCode();
        }

        Clock.Set(until);
        Provider.AddInvoice(remote.Id, until);
        await ReconcileAsync(owner, id);
        Assert.Equal(4, (await ContractAsync(owner)).PlanId);
        using var scope = Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var staff = await db.EnterpriseMemberships.IgnoreQueryFilters().Where(Membership =>
            Membership.EnterpriseId == owner.EnterpriseId && Membership.KindId != 1).ToListAsync();
        Assert.True(staff.Single(Membership => Membership.Id == staffA.MembershipId).IsContractBlocked);
        Assert.False(staff.Single(Membership => Membership.Id == staffB.MembershipId).IsContractBlocked);
    }

    [PostgresFact]
    public async Task Seven_day_grace_is_fixed_and_provider_outage_cannot_keep_paid_features_after_deadline()
    {
        Reset();
        var (owner, id, remote) = await ActiveAsync(2);
        var staff = await Api.CreateStaffAsync(owner.EnterpriseId);
        var until = (await SubscriptionAsync(id)).PaidUntil!.Value;
        Provider.AddInvoice(remote.Id, until, approved: false);
        Clock.Set(until.AddDays(6));
        await ReconcileAsync(owner, id);
        Assert.Equal(2, (await ContractAsync(owner)).PlanId);
        using (var client = Api.Client(await Api.FreshTokenAsync(owner)))
        {
            var summary = await client.GetFromJsonAsync<PlatformBillingSummary>(BillingPath);
            Assert.Equal("past_due", summary!.Status);
            Assert.Equal(until.AddDays(7), summary.GraceEndsAt);
            Assert.Contains(summary.Notices, Notice => Notice.Code == "Billing.PaymentDue");
        }

        var pendingChange = await RequestAsync(owner, 4);
        using (var client = Api.Client(await Api.FreshTokenAsync(owner)))
        {
            using var denied = await client.PostAsJsonAsync(BillingPath + "/checkout", new { contractChangeId = pendingChange });
            Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);
        }

        Clock.Set(until.AddDays(7));
        Provider.Unavailable = true;
        await Assert.ThrowsAsync<HttpRequestException>(() => ReconcileAsync(owner, id));
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);
        using (var scope = Api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            Assert.True((await db.EnterpriseMemberships.IgnoreQueryFilters().SingleAsync(Membership => Membership.Id == staff.MembershipId)).IsContractBlocked);
            Assert.Equal(until, (await SubscriptionAsync(id)).PaidUntil);
        }

        Provider.Unavailable = false;
        Provider.ApproveLastInvoice(remote.Id, Clock.GetUtcNow().UtcDateTime);
        await ReconcileAsync(owner, id);
        Assert.Equal(2, (await ContractAsync(owner)).PlanId);
        Assert.Equal(until.AddMonths(1), (await SubscriptionAsync(id)).PaidUntil);
    }

    [PostgresFact]
    public async Task Unpaid_initial_authorization_can_be_canceled_before_choosing_another_paid_offer()
    {
        Reset();
        var owner = await Api.CreateStoreAsync();
        var summary = await StartAsync(owner, await RequestAsync(owner, 4));
        await StartAsync(owner, await RequestAsync(owner, 1, []));
        Assert.NotNull((await SubscriptionAsync(summary.SubscriptionId!.Value)).ClosedAt);
        var replacement = await StartAsync(owner, await RequestAsync(owner, 2));
        Assert.NotEqual(summary.SubscriptionId, replacement.SubscriptionId);
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);
        Assert.Equal(2, Provider.SubscriptionPosts);
    }

    [PostgresFact]
    public async Task Free_downgrade_cancels_future_debits_but_keeps_paid_period_without_extra_grace()
    {
        Reset();
        var (owner, id, remote) = await ActiveAsync(2);
        var until = (await SubscriptionAsync(id)).PaidUntil!.Value;
        var change = await RequestAsync(owner, 1, []);
        await StartAsync(owner, change);
        Assert.Equal("cancelled", Provider.Subscriptions[remote.Id].Status);
        Assert.Equal(2, (await ContractAsync(owner)).PlanId);
        Clock.Set(until);
        await ReconcileAsync(owner, id);
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);
        Assert.NotNull((await SubscriptionAsync(id)).ClosedAt);
    }

    [PostgresFact]
    public async Task Lost_subscription_response_and_concurrent_retries_create_only_one_remote_subscription()
    {
        Reset();
        var owner = await Api.CreateStoreAsync();
        var change = await RequestAsync(owner, 4);
        Provider.LoseNextSubscriptionResponse = true;
        var summary = await StartAsync(owner, change);
        Assert.Equal(1, Provider.SubscriptionPosts);
        await Task.WhenAll(ReconcileAsync(owner, summary.SubscriptionId!.Value), ReconcileAsync(owner, summary.SubscriptionId.Value));
        Assert.NotNull((await SubscriptionAsync(summary.SubscriptionId.Value)).RemoteId);
        Assert.Equal(1, Provider.SubscriptionPosts);
        await StartAsync(owner, change);
        Assert.Equal(1, Provider.SubscriptionPosts);
    }

    [PostgresFact]
    public async Task Abandoned_upgrade_expires_without_blocking_another_choice_and_late_payment_needs_review()
    {
        Reset();
        var (owner, id, _) = await ActiveAsync(4);
        await StartAsync(owner, await RequestAsync(owner, 2));
        var invoice = await UpgradeInvoiceAsync(id);
        Clock.Set(invoice.CheckoutExpiresAt);
        await ReconcileAsync(owner, id);
        Assert.Null((await SubscriptionAsync(id)).PendingOfferId);
        Assert.Null((await SubscriptionAsync(id)).ReviewCode);
        Assert.Equal("expired", (await UpgradeInvoiceAsync(id)).Status);

        await StartAsync(owner, await RequestAsync(owner, 3));
        Provider.PayUpgrade(invoice.Reference, invoice.Amount, Clock.GetUtcNow().UtcDateTime);
        await ReconcileAsync(owner, id);
        Assert.Equal(4, (await ContractAsync(owner)).PlanId);
        Assert.Equal("Billing.LateUpgradePayment", (await SubscriptionAsync(id)).ReviewCode);
    }

    [PostgresFact]
    public async Task Wrong_amount_or_refunded_first_invoice_never_grants_paid_access()
    {
        Reset();
        var owner = await Api.CreateStoreAsync();
        var summary = await StartAsync(owner, await RequestAsync(owner, 4));
        var remote = await RemoteAsync(summary.SubscriptionId!.Value);
        Provider.Subscriptions[remote.Id] = remote with { Status = "authorized" };
        var payment = Provider.AddInvoice(remote.Id, Clock.GetUtcNow().UtcDateTime.AddSeconds(-1));
        Provider.Payments[payment.Id] = payment with { Amount = 1m };
        await ReconcileAsync(owner, summary.SubscriptionId.Value);
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);
        Assert.NotNull((await SubscriptionAsync(summary.SubscriptionId.Value)).ReviewCode);

        var other = await Api.CreateStoreAsync();
        var otherSummary = await StartAsync(other, await RequestAsync(other, 4));
        var otherRemote = await RemoteAsync(otherSummary.SubscriptionId!.Value);
        Provider.Subscriptions[otherRemote.Id] = otherRemote with { Status = "authorized" };
        var reversed = Provider.AddInvoice(otherRemote.Id, Clock.GetUtcNow().UtcDateTime.AddSeconds(-1));
        Provider.Payments[reversed.Id] = reversed with { RefundedAmount = reversed.Amount };
        await ReconcileAsync(other, otherSummary.SubscriptionId.Value);
        Assert.Equal(1, (await ContractAsync(other)).PlanId);
    }

    [PostgresFact]
    public async Task Billing_is_owner_only_tenant_scoped_and_generic_identity_cannot_write_financial_entities()
    {
        Reset();
        var (owner, id, _) = await ActiveAsync(2);
        var staff = await Api.CreateStaffAsync(owner.EnterpriseId);
        using (var client = Api.Client(staff.Token))
        {
            using var response = await client.GetAsync(BillingPath);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        var other = await Api.CreateStoreAsync();
        var offerId = await RequestAsync(other, 4);
        using (var client = Api.Client(await Api.FreshTokenAsync(owner)))
        {
            using var response = await client.PostAsJsonAsync(BillingPath + "/checkout", new { contractChangeId = offerId });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        using var scope = Api.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var subscription = await db.Set<PlatformSubscription>().IgnoreQueryFilters().SingleAsync(Subscription => Subscription.Id == id);
        subscription.Review("attempted-generic-write");
        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => db.SaveChangesAsync());
        Assert.Equal("Billing.UseWorkflow", failure.Code);
    }

    [PostgresFact]
    public async Task Webhook_requires_platform_signature_accepts_alphanumeric_subscription_id_and_deduplicates_delivery()
    {
        Reset();
        var owner = await Api.CreateStoreAsync();
        var summary = await StartAsync(owner, await RequestAsync(owner, 4));
        var remote = await RemoteAsync(summary.SubscriptionId!.Value);
        Provider.Subscriptions[remote.Id] = remote with { Status = "authorized" };
        Provider.AddInvoice(remote.Id, Clock.GetUtcNow().UtcDateTime.AddSeconds(-1));
        using var client = Api.Client();
        var path = "/api/platform-billing/mercado-pago/webhook?data.id=" + remote.Id;
        var payload = new { type = "subscription_preapproval", data = new { id = remote.Id } };
        using (var denied = await client.PostAsJsonAsync(path, payload))
            Assert.Equal(HttpStatusCode.BadRequest, denied.StatusCode);

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var manifest = $"id:{remote.Id};request-id:delivery-1;ts:{timestamp};";
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes("fake-billing-secret"), Encoding.UTF8.GetBytes(manifest));
        client.DefaultRequestHeaders.Add("x-request-id", "delivery-1");
        client.DefaultRequestHeaders.Add("x-signature", $"ts={timestamp},v1={Convert.ToHexString(hash).ToLowerInvariant()}");
        using (var accepted = await client.PostAsJsonAsync(path, payload))
            accepted.EnsureSuccessStatusCode();

        using (var duplicate = await client.PostAsJsonAsync(path, payload))
            duplicate.EnsureSuccessStatusCode();

        using var scope = Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var deliveries = await db.Set<PlatformBillingNotification>().Where(Notification => Notification.ResourceId == remote.Id).ToListAsync();
        Assert.Single(deliveries);
        Assert.Equal(1, (await ContractAsync(owner)).PlanId);
        await scope.ServiceProvider.GetRequiredService<PlatformBillingNotifications>().ProcessAsync(deliveries[0].Id, default);
        Assert.Equal(4, (await ContractAsync(owner)).PlanId);
    }

    [Fact]
    public void Calendar_month_and_grace_boundaries_have_no_sliding_extension()
    {
        var end = new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal("active", PlatformBillingPolicy.Status(end, false, false, end.AddTicks(-1)));
        Assert.Equal("past_due", PlatformBillingPolicy.Status(end, false, false, end));
        Assert.Equal("past_due", PlatformBillingPolicy.Status(end, false, true, end.AddDays(7).AddTicks(-1)));
        Assert.Equal("expired", PlatformBillingPolicy.Status(end, false, true, end.AddDays(7)));
        var anchor = new DateTime(2027, 1, 31, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2027, 2, 28, 12, 0, 0, DateTimeKind.Utc), PlatformBillingPolicy.PeriodEnd(anchor, null));
        Assert.Equal(new DateTime(2027, 3, 31, 12, 0, 0, DateTimeKind.Utc), PlatformBillingPolicy.PeriodEnd(anchor.AddMonths(1), anchor));
    }

    private void Reset()
    {
        Clock.Set(DateTime.UtcNow.AddMinutes(-1));
        Provider.SubscriptionPosts = 0;
        Provider.ChargePosts = 0;
        Provider.Unavailable = false;
        Provider.LoseNextSubscriptionResponse = false;
    }

    private async Task<long> RequestAsync(StoreActor owner, long planId, IReadOnlyList<long>? retained = null)
    {
        var contract = await ContractAsync(owner);
        using var client = Api.Client(await Api.FreshTokenAsync(owner));
        using var response = await client.PostAsJsonAsync("/api/enterprises/current/contract/requests", new
        {
            idempotencyKey = Guid.NewGuid().ToString("N"), planId,
            expectedContractRevision = contract.ContractRevision, catalogVersion = 1, retainedMembershipIds = retained
        });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.GetProperty("id").GetInt64();
    }

    private async Task<PlatformBillingSummary> StartAsync(StoreActor owner, long change)
    {
        using var client = Api.Client(await Api.FreshTokenAsync(owner));
        using var response = await client.PostAsJsonAsync(BillingPath + "/checkout", new { contractChangeId = change });
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, text);

        return JsonSerializer.Deserialize<PlatformBillingSummary>(text, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    private async Task<(StoreActor Owner, long Id, PlatformRecurringSubscription Remote)> ActiveAsync(long plan)
    {
        var owner = await Api.CreateStoreAsync();
        var summary = await StartAsync(owner, await RequestAsync(owner, plan));
        var id = summary.SubscriptionId!.Value;
        var remote = await RemoteAsync(id);
        Provider.Subscriptions[remote.Id] = remote with { Status = "authorized" };
        Provider.AddInvoice(remote.Id, Clock.GetUtcNow().UtcDateTime.AddSeconds(-1));
        await ReconcileAsync(owner, id);
        Assert.Equal(plan, (await ContractAsync(owner)).PlanId);

        return (owner, id, remote);
    }

    private async Task ReconcileAsync(StoreActor owner, long id)
    {
        using var scope = Api.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PlatformBillingService>().ReconcileAsync(owner.EnterpriseId, id, default);
    }

    private async Task<Enterprise> ContractAsync(StoreActor owner)
    {
        using var scope = Api.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().Enterprises.IgnoreQueryFilters()
            .AsNoTracking().SingleAsync(Enterprise => Enterprise.Id == owner.EnterpriseId);
    }

    private async Task<PlatformSubscription> SubscriptionAsync(long id)
    {
        using var scope = Api.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().Set<PlatformSubscription>()
            .IgnoreQueryFilters().AsNoTracking().SingleAsync(Subscription => Subscription.Id == id);
    }

    private async Task<PlatformRecurringSubscription> RemoteAsync(long id)
    {
        var subscription = await SubscriptionAsync(id);

        return Provider.Subscriptions[subscription.RemoteId!];
    }

    private async Task<PlatformBillingInvoice> UpgradeInvoiceAsync(long id)
    {
        using var scope = Api.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().Set<PlatformBillingInvoice>()
            .IgnoreQueryFilters().AsNoTracking().SingleAsync(Invoice => Invoice.SubscriptionId == id && Invoice.IsUpgrade);
    }
}

public sealed class BillingClock : TimeProvider
{
    private DateTimeOffset now = DateTimeOffset.UtcNow;

    public void Set(DateTime utc)
    {
        now = new DateTimeOffset(utc);
    }

    public override DateTimeOffset GetUtcNow()
    {
        return now;
    }
}

public sealed class BillingProviderFake : IPlatformSubscriptionProvider, IPlatformBillingProvider
{
    public ConcurrentDictionary<string, PlatformRecurringSubscription> Subscriptions { get; } = new();
    public ConcurrentDictionary<string, PlatformRecurringInvoice> Invoices { get; } = new();
    public ConcurrentDictionary<string, PlatformBillingPayment> Payments { get; } = new();
    public ConcurrentDictionary<string, PlatformBillingCheckout> Checkouts { get; } = new();
    public int SubscriptionPosts;
    public int ChargePosts;
    public bool Unavailable;
    public bool LoseNextSubscriptionResponse;
    private long sequence = 1000;

    public Task<PlatformRecurringSubscription> CreateAsync(string reference, string payerEmail, decimal amount, CancellationToken ct)
    {
        Interlocked.Increment(ref SubscriptionPosts);
        var subscription = new PlatformRecurringSubscription(Guid.NewGuid().ToString("N"), reference, "123", amount,
            "BRL", "pending", "https://www.mercadopago.com.br/subscriptions/checkout");
        Subscriptions[subscription.Id] = subscription;

        if (LoseNextSubscriptionResponse)
        {
            LoseNextSubscriptionResponse = false;

            throw new HttpRequestException("Simulated lost response.");
        }

        return Task.FromResult(subscription);
    }

    public Task<PlatformRecurringSubscription?> RecoverAsync(string reference, CancellationToken ct)
    {
        Check();

        return Task.FromResult(Subscriptions.Values.SingleOrDefault(Subscription => Subscription.Reference == reference));
    }

    public Task<PlatformRecurringSubscription> GetAsync(string id, CancellationToken ct)
    {
        Check();

        return Task.FromResult(Subscriptions[id]);
    }

    public Task<PlatformRecurringSubscription> UpdateAsync(string id, decimal amount, bool cancel, CancellationToken ct)
    {
        Check();
        var subscription = Subscriptions[id] with { MonthlyAmount = cancel ? Subscriptions[id].MonthlyAmount : amount,
            Status = cancel ? "cancelled" : Subscriptions[id].Status };
        Subscriptions[id] = subscription;

        return Task.FromResult(subscription);
    }

    public Task<IReadOnlyList<PlatformRecurringInvoice>> InvoicesAsync(string id, CancellationToken ct)
    {
        Check();

        return Task.FromResult<IReadOnlyList<PlatformRecurringInvoice>>(Invoices.Values.Where(Invoice => Invoice.SubscriptionId == id).ToArray());
    }

    public Task<PlatformRecurringInvoice> InvoiceAsync(string id, CancellationToken ct)
    {
        return Task.FromResult(Invoices[id]);
    }

    public Task ValidateAccountAsync(PlatformBillingCharge charge, CancellationToken ct)
    {
        Check();

        return Task.CompletedTask;
    }

    public Task<PlatformBillingCheckout> CreateAsync(PlatformBillingCharge charge, CancellationToken ct)
    {
        Interlocked.Increment(ref ChargePosts);
        var checkout = new PlatformBillingCheckout("preference-" + charge.ExternalReference, "https://www.mercadopago.com.br/checkout");
        Checkouts[charge.ExternalReference] = checkout;

        return Task.FromResult(checkout);
    }

    public Task<PlatformBillingCheckout?> RecoverAsync(PlatformBillingCharge charge, CancellationToken ct)
    {
        Checkouts.TryGetValue(charge.ExternalReference, out var checkout);

        return Task.FromResult(checkout);
    }

    public Task<PlatformBillingPayment> GetPaymentAsync(string id, CancellationToken ct)
    {
        Check();

        return Task.FromResult(Payments[id]);
    }

    public Task<IReadOnlyList<PlatformBillingPayment>> SearchPaymentsAsync(string reference, CancellationToken ct)
    {
        Check();

        return Task.FromResult<IReadOnlyList<PlatformBillingPayment>>(Payments.Values.Where(Payment => Payment.ExternalReference == reference).ToArray());
    }

    public PlatformBillingPayment AddInvoice(string subscriptionId, DateTime start, bool approved = true)
    {
        var subscription = Subscriptions[subscriptionId];
        var invoiceId = Interlocked.Increment(ref sequence).ToString();
        var payment = PayUpgrade(subscription.Reference, subscription.MonthlyAmount, start, approved);
        Invoices[invoiceId] = new PlatformRecurringInvoice(invoiceId, subscriptionId, subscription.MonthlyAmount, "BRL", start, payment.Id);

        return payment;
    }

    public PlatformBillingPayment PayUpgrade(string reference, decimal amount, DateTime now, bool approved = true)
    {
        var id = Interlocked.Increment(ref sequence).ToString();
        var payment = new PlatformBillingPayment(id, reference, "123", amount, "BRL", approved ? "approved" : "rejected", 0, now);
        Payments[id] = payment;

        return payment;
    }

    public void ApproveLastInvoice(string subscriptionId, DateTime updatedAt)
    {
        var invoice = Invoices.Values.Where(Invoice => Invoice.SubscriptionId == subscriptionId).OrderByDescending(Invoice => Invoice.DebitDate).First();
        Payments[invoice.PaymentId!] = Payments[invoice.PaymentId!] with { Status = "approved", UpdatedAt = updatedAt };
    }

    private void Check()
    {
        if (Unavailable)
            throw new HttpRequestException("Simulated provider outage.");
    }
}
