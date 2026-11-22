using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using DropCommerce.Application.Security;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Features.Commands;
using DropCommerce.Application.Features.Commands;
using MediatR;
using DropCommerce.Application.Contracts;
using System.Net.Http.Json;
using DropCommerce.Domain.Entity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Payments;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Payments;
using StoreCommerce.Security.Services;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class DropPurchaseTests(ContractsApiFixture store)
{
    [Fact]
    public void Cpf_identity_is_validated_and_scoped_to_the_event_without_storing_plaintext()
    {
        const string secret = "test-only-private-key-with-at-least-32-characters";
        var hash = DropPurchaseIdentity.Create(secret, 1, 10, "529.982.247-25");
        Assert.Equal(64, hash.Length);
        Assert.DoesNotContain("52998224725", hash);
        Assert.Equal(hash, DropPurchaseIdentity.Create(secret, 1, 10, "52998224725"));
        Assert.NotEqual(hash, DropPurchaseIdentity.Create(secret, 1, 11, "52998224725"));
        Assert.NotEqual(hash, DropPurchaseIdentity.Create(secret, 2, 10, "52998224725"));
        Assert.Throws<SecurityFailure>(() => DropPurchaseIdentity.Create(secret, 1, 10, "11111111111"));
        Assert.Throws<SecurityFailure>(() => DropPurchaseIdentity.Create(secret, 1, 10, "52998224726"));
        Assert.Throws<SecurityFailure>(() => DropPurchaseIdentity.Create("short", 1, 10, "52998224725"));
    }

    [PostgresFact]
    public async Task Variants_and_accounts_share_cpf_allowance_and_deadline_does_not_extend()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        var clock = new PurchaseClock();
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner, clock);
        var (product, participation, dropEvent) = await ProductEventAsync(drop, owner);
        var buyer = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, buyer.Token);
        var queue = drop.Services.GetRequiredService<IMediator>();
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "52998224725" }), default);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        var session = await db.QueueSessions.IgnoreQueryFilters().SingleAsync();
        var deadline = session.ExpiresAt;
        Assert.Equal(clock.UtcNow.AddMinutes(15), deadline);
        clock.Advance(TimeSpan.FromMinutes(2));
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "529.982.247-25" }), default);
        Assert.Equal(deadline, (await db.QueueSessions.IgnoreQueryFilters().SingleAsync()).ExpiresAt);
        var variants = product.Variants.OrderBy(Variant => Variant.Id).ToArray();
        var request = new ReserveDropPurchase(participation.Id, session.Id, "two-colors", [new(variants[0].Id, 2), new(variants[1].Id, 1)]);
        var reservation = await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, request), default);
        Assert.Equal(3, reservation.Quantity);
        Assert.Equal(deadline, reservation.ExpiresAt);
        Assert.Equal(32, reservation.TotalAmount);
        var repeat = await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, request), default);
        Assert.Equal(reservation.Id, repeat.Id);
        Assert.Equal(3, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved));
        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, session.Id, "one-more", [new(variants[0].Id, 1)])), default));
        Assert.Equal("DropPurchase.PurchaseLimit", failure.Code);
        db.ChangeTracker.Clear();
        var second = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, second.Token);
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "52998224725" }), default);
        var secondSession = await db.QueueSessions.IgnoreQueryFilters().OrderByDescending(Session => Session.Id).FirstAsync();
        failure = await Assert.ThrowsAsync<SecurityFailure>(() => queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, secondSession.Id, "another-account", [new(variants[1].Id, 1)])), default));
        Assert.Equal("DropPurchase.PurchaseLimit", failure.Code);
        Assert.Equal(1, await db.DropReservations.IgnoreQueryFilters().CountAsync());
    }

    [PostgresFact]
    public async Task Expiration_releases_units_and_requeues_behind_waiting_buyers()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        var clock = new PurchaseClock();
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner, clock, capacity: 1);
        var (product, participation, dropEvent) = await ProductEventAsync(drop, owner);
        var buyer = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, buyer.Token);
        var queue = drop.Services.GetRequiredService<IMediator>();
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "52998224725" }), default);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        var session = await db.QueueSessions.IgnoreQueryFilters().SingleAsync();
        var reservation = await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, session.Id, "expires", [new(product.Variants.First().Id, 3)])), default);
        var second = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, second.Token);
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "11144477735" }), default);
        Assert.Equal(1, await db.QueueSessions.IgnoreQueryFilters().CountAsync());
        clock.Advance(TimeSpan.FromMinutes(15));
        // O worker admite sem sessão HTTP de comprador ou dono.
        drop.Services.GetRequiredService<IHttpContextAccessor>().HttpContext!.User = new System.Security.Claims.ClaimsPrincipal();
        await queue.SendRequiredAsync(new AdmitDropQueueCommand(owner.EnterpriseId, dropEvent.Id), default);
        Assert.Equal(0, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved));
        Assert.Equal(10, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        Assert.Equal(3, (await db.DropReservations.IgnoreQueryFilters().SingleAsync(Reservation => Reservation.Id == reservation.Id)).DropReservationStatusId);
        var entries = await db.QueueEntries.IgnoreQueryFilters().OrderBy(Entry => Entry.Id).ToListAsync();
        Assert.True(entries[0].Position > entries[1].Position);
        Assert.Equal(1, entries[0].QueueEntryStatusId);
        Assert.Equal(2, entries[1].QueueEntryStatusId);
        var next = await db.QueueSessions.IgnoreQueryFilters().OrderByDescending(Session => Session.Id).FirstAsync();
        Assert.Equal(clock.UtcNow.AddMinutes(15), next.ExpiresAt);
    }

    [PostgresFact]
    public async Task New_event_resets_cpf_quota_but_shares_physical_stock()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        var clock = new PurchaseClock();
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner, clock);
        var (product, participation, firstEvent) = await ProductEventAsync(drop, owner);
        var secondEvent = await CreateEventAsync(drop, owner);
        var secondParticipation = await drop.Services.GetRequiredService<IMediator>().SendRequiredAsync(new AssociateDropCatalogProductCommand(product.Id, new(secondEvent.Id, 3)), default);
        await PublishAsync(drop, secondEvent);
        var buyer = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, buyer.Token);
        var queue = drop.Services.GetRequiredService<IMediator>();
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        await queue.SendRequiredAsync(new JoinDropQueueCommand(firstEvent.Id, new() { CPF = "52998224725" }), default);
        var first = await db.QueueSessions.IgnoreQueryFilters().SingleAsync();
        await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(firstEvent.Id, new(participation.Id, first.Id, "first", [new(product.Variants.First().Id, 3)])), default);
        await queue.SendRequiredAsync(new JoinDropQueueCommand(secondEvent.Id, new() { CPF = "52998224725" }), default);
        var second = await db.QueueSessions.IgnoreQueryFilters().OrderByDescending(Session => Session.Id).FirstAsync();
        var shortage = await Assert.ThrowsAsync<SecurityFailure>(() => queue.SendRequiredAsync(new ReserveDropPurchaseCommand(secondEvent.Id, new(secondParticipation.Id, second.Id, "second", [new(product.Variants.First().Id, 3)])), default));
        Assert.Equal("DropPurchase.InsufficientStock", shortage.Code);
        db.ChangeTracker.Clear();
        var reservation = await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(secondEvent.Id, new(secondParticipation.Id, second.Id, "second-color", [new(product.Variants.Last().Id, 3)])), default);
        Assert.Equal(3, reservation.Quantity);
        Assert.Equal(6, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved));
    }

    [PostgresFact]
    public async Task Concurrent_reservations_cannot_oversubscribe_cpf_or_variant_balance()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        var clock = new PurchaseClock();
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner, clock);
        var (product, participation, dropEvent) = await ProductEventAsync(drop, owner);
        var buyer = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, buyer.Token);
        var queue = drop.Services.GetRequiredService<IMediator>();
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "52998224725" }), default);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        var session = await db.QueueSessions.IgnoreQueryFilters().SingleAsync();

        async Task<bool> ReserveAsync(string key, int quantity)
        {
            using var scope = drop.Services.CreateScope();

            try
            {
                await scope.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, session.Id, key, [new(product.Variants.First().Id, quantity)])), default);

                return true;
            }
            catch (SecurityFailure failure) when (failure.Code is "DropPurchase.PurchaseLimit" or "DropPurchase.InsufficientStock")
            {
                return false;
            }
        }

        var outcomes = await Task.WhenAll(ReserveAsync("concurrent-a", 3), ReserveAsync("concurrent-b", 2));
        Assert.Single(outcomes, Outcome => Outcome);
        Assert.Equal(1, await db.DropReservations.IgnoreQueryFilters().CountAsync());
        Assert.InRange(await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved), 2, 3);
        Assert.Equal(10, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        var variant = await db.Set<DropCatalogVariant>().FirstAsync();
        variant.Reserve(1);
        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => db.SaveChangesAsync());
        Assert.Equal("DropStock.UseWorkflow", failure.Code);
    }

    [PostgresFact]
    public async Task Checkout_uses_seller_bridge_and_approved_payment_consumes_drop_stock_once()
    {
        var gateway = new TestDropGateway { LoseFirstResponse = true };
        var source = new ContractsApiFixture { ConfigureServices = Services => Services.AddSingleton<IDropSaleGateway>(gateway) };
        await source.InitializeAsync();

        try
        {
            var owner = await source.CreateStoreAsync(planId: 5);
            var clock = new PurchaseClock();
            await using var drop = new DropAppearanceTestScope();
            await drop.InitializeAsync(source, owner, clock);
            var (product, participation, dropEvent) = await ProductEventAsync(drop, owner);
            var buyer = await source.CreateCustomerAsync(owner.EnterpriseId);
            SetBuyer(source, drop, buyer.Token);
            var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
            var queue = drop.Services.GetRequiredService<IMediator>();
            await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "52998224725" }), default);
            var session = await db.QueueSessions.IgnoreQueryFilters().SingleAsync();
            var reservation = await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, session.Id, "checkout", [new(product.Variants.First().Id, 2)])), default);
            var checkout = drop.Services.GetRequiredService<IMediator>();
            await Assert.ThrowsAnyAsync<Exception>(() => checkout.SendRequiredAsync(new CheckoutDropPurchaseCommand(reservation.Id), default));
            db.ChangeTracker.Clear();
            await checkout.SendRequiredAsync(new CheckoutDropPurchaseCommand(reservation.Id), default);
            await checkout.SendRequiredAsync(new CheckoutDropPurchaseCommand(reservation.Id), default);
            Assert.Equal(1, gateway.Creates);
            Assert.Equal(1, gateway.Recoveries);
            var order = await db.DropOrders.IgnoreQueryFilters().SingleAsync();
            Assert.NotNull(order.PaymentCheckoutUrl);
            Assert.Equal(2, await db.DropOrderItems.IgnoreQueryFilters().SumAsync(Item => Item.Quantity));
            using var sourceScope = source.Services.CreateScope();
            var sourceDb = sourceScope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            Assert.Equal(0, await sourceDb.Orders.IgnoreQueryFilters().CountAsync(Order => Order.EnterpriseId == owner.EnterpriseId));
            Assert.Equal(1, await sourceDb.Set<StoreCommerce.Domain.Entity.DropSalePaymentIntent>().IgnoreQueryFilters().CountAsync());
            // A perda do módulo não cancela uma reserva que ainda está dentro do prazo original.
            await source.ChangeContractAsync(owner.EnterpriseId, 2, null);
            await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
            gateway.Status = "approved";
            await checkout.SendRequiredAsync(new ReconcileDropPurchaseCommand(owner.EnterpriseId, order.Id), default);
            await checkout.SendRequiredAsync(new ReconcileDropPurchaseCommand(owner.EnterpriseId, order.Id), default);
            order = await db.DropOrders.IgnoreQueryFilters().SingleAsync();
            Assert.Equal(2, order.DropOrderPaymentStatusId);
            Assert.Equal(2, order.DropOrderStatusId);
            Assert.Null(order.FinancialReviewCode);
            Assert.Equal(8, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
            Assert.Equal(0, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved));
            Assert.Equal(2, (await db.DropProducts.IgnoreQueryFilters().SingleAsync()).UnitsSold);
            Assert.Equal(1, gateway.Creates);
            var failure = await Assert.ThrowsAsync<SecurityFailure>(() => queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, session.Id, "blocked", [new(product.Variants.First().Id, 1)])), default));
            Assert.Equal("DropPurchase.InvalidReservation", failure.Code);
        }
        finally
        {
            await source.DisposeAsync();
        }
    }

    [PostgresFact]
    public async Task Financial_review_reopens_fulfillment_and_old_ack_cannot_unlock_account_deletion()
    {
        var gateway = new TestDropGateway();
        var source = new ContractsApiFixture { ConfigureServices = Services => Services.AddSingleton<IDropSaleGateway>(gateway) };
        await source.InitializeAsync();

        try
        {
            var owner = await source.CreateStoreAsync(planId: 5);
            var buyer = await source.CreateCustomerAsync(owner.EnterpriseId);
            using var scope = source.Services.CreateScope();
            var principal = scope.ServiceProvider.GetRequiredService<JwtSessionService>().ValidateSignature(buyer.Token);
            var customerId = SharedCommerce.Application.Security.SecurityNames.Id(principal, SharedCommerce.Application.Security.SecurityNames.Customer);
            var payments = scope.ServiceProvider.GetRequiredService<IMediator>();
            var request = new StartDropSalePayment(Guid.NewGuid(), 100, customerId, 20, "BRL", DateTime.UtcNow.AddMinutes(14));
            var receipt = await payments.SendRequiredAsync(new StartDropSalePaymentCommand(owner.EnterpriseId, request), default);
            var security = source.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SharedCommerce.Application.Options.SecurityOptions>>().Value;
            using var service = source.Client();
            service.DefaultRequestHeaders.Add("X-Service-Key", security.InternalServiceKey);
            service.DefaultRequestHeaders.Add("X-Enterprise-Id", owner.EnterpriseId.ToString());
            using var closed = await service.PostAsJsonAsync($"/api/internal/drop-sale-payments/{request.OperationId}/ack",
                new { receipt.Revision, fulfillmentClosed = true, requiresReview = false });
            closed.EnsureSuccessStatusCode();
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            db.ChangeTracker.Clear();
            gateway.Status = "approved";
            receipt = await payments.SendRequiredAsync(new ReconcileDropSalePaymentCommand(owner.EnterpriseId, request.OperationId), default);
            var eligibility = await scope.ServiceProvider.GetRequiredService<IAccountDeletionStore>()
                .InspectCustomerAsync(owner.EnterpriseId, customerId, DateTime.UtcNow, default);
            // O recibo aprovado ainda não foi confirmado pelo DROP, logo continua bloqueado.
            Assert.False(eligibility.CanDelete);
            using var review = await service.PostAsJsonAsync($"/api/internal/drop-sale-payments/{request.OperationId}/ack",
                new { receipt.Revision, fulfillmentClosed = false, requiresReview = true });
            review.EnsureSuccessStatusCode();
            using var stale = await service.PostAsJsonAsync($"/api/internal/drop-sale-payments/{request.OperationId}/ack",
                new { revision = 1, fulfillmentClosed = true, requiresReview = false });
            stale.EnsureSuccessStatusCode();
            db.ChangeTracker.Clear();
            var intent = await db.Set<StoreCommerce.Domain.Entity.DropSalePaymentIntent>().IgnoreQueryFilters().SingleAsync();
            Assert.True(intent.FulfillmentReviewRequired);
            Assert.False(intent.FulfillmentClosed);
            eligibility = await scope.ServiceProvider.GetRequiredService<IAccountDeletionStore>()
                .InspectCustomerAsync(owner.EnterpriseId, customerId, DateTime.UtcNow, default);
            Assert.False(eligibility.CanDelete);
            Assert.Contains(eligibility.Blockers, Blocker => Blocker.Code == "AccountDeletion.PendingDropSales");
        }
        finally
        {
            await source.DisposeAsync();
        }
    }

    [PostgresFact]
    public async Task Late_approval_and_mismatched_receipts_never_consume_released_stock()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        var clock = new PurchaseClock();
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner, clock);
        var (product, participation, dropEvent) = await ProductEventAsync(drop, owner);
        var buyer = await store.CreateCustomerAsync(owner.EnterpriseId);
        SetBuyer(store, drop, buyer.Token);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        var queue = drop.Services.GetRequiredService<IMediator>();
        await queue.SendRequiredAsync(new JoinDropQueueCommand(dropEvent.Id, new() { CPF = "52998224725" }), default);
        var session = await db.QueueSessions.IgnoreQueryFilters().SingleAsync();
        var reservation = await queue.SendRequiredAsync(new ReserveDropPurchaseCommand(dropEvent.Id, new(participation.Id, session.Id, "late", [new(product.Variants.First().Id, 1)])), default);
        // O provedor padrão está desabilitado neste fixture: a intenção local sobrevive ao erro externo.
        var checkout = drop.Services.GetRequiredService<IMediator>();
        await Assert.ThrowsAnyAsync<Exception>(() => checkout.SendRequiredAsync(new CheckoutDropPurchaseCommand(reservation.Id), default));
        db.ChangeTracker.Clear();
        var order = await db.DropOrders.IgnoreQueryFilters().SingleAsync();
        var receipt = new DropSalePaymentReceipt(order.PaymentOperationId!.Value, owner.EnterpriseId, order.Id,
            order.CustomerId, order.TotalAmount, "BRL", "approved", null, "test-payment", 3);
        await Assert.ThrowsAsync<SecurityFailure>(() => checkout.SendRequiredAsync(new ApplyDropPaymentReceiptCommand(receipt with { Amount = receipt.Amount + 1 }), default));
        Assert.Equal(1, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved));
        clock.Advance(TimeSpan.FromMinutes(15));
        await checkout.SendRequiredAsync(new ApplyDropPaymentReceiptCommand(receipt), default);
        await checkout.SendRequiredAsync(new ApplyDropPaymentReceiptCommand(receipt), default);
        order = await db.DropOrders.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("DropPayment.LateApproval", order.FinancialReviewCode);
        Assert.Equal(6, order.DropOrderStatusId);
        Assert.Equal("approved", order.PaymentResolution);
        Assert.Equal(10, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        Assert.Equal(0, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityReserved));
        Assert.Equal(0, (await db.DropProducts.IgnoreQueryFilters().SingleAsync()).UnitsSold);
    }

    private static void SetBuyer(ContractsApiFixture source, DropAppearanceTestScope drop, string token)
    {
        using var scope = source.Services.CreateScope();
        drop.Services.GetRequiredService<IHttpContextAccessor>().HttpContext!.User =
            scope.ServiceProvider.GetRequiredService<JwtSessionService>().ValidateSignature(token);
    }

    private static async Task<(DropCatalogProduct Product, DropProduct Participation, DropEvent Event)> ProductEventAsync(DropAppearanceTestScope scope, StoreActor owner)
    {
        var catalog = scope.Services.GetRequiredService<IMediator>();
        var product = await catalog.SendRequiredAsync(new CreateDropCatalogProductCommand(new("Camiseta", "Produto próprio", [new("BLUE", new() { ["color"] = "blue" }, 10, 5),
            new("BLACK", new() { ["color"] = "black" }, 12, 5)])), default);
        var dropEvent = await CreateEventAsync(scope, owner);
        var participation = await catalog.SendRequiredAsync(new AssociateDropCatalogProductCommand(product.Id, new(dropEvent.Id, 3)), default);
        await PublishAsync(scope, dropEvent);

        return (product, participation, dropEvent);
    }

    private static async Task<DropEvent> CreateEventAsync(DropAppearanceTestScope scope, StoreActor owner)
    {
        var now = DateTime.UtcNow;
        var drop = DropEvent.Create(owner.EnterpriseId, null, "Evento", "test-" + Guid.NewGuid().ToString("N"), "Descrição", "image", "banner",
            1, 10, 0, 0, 10, false, false, now.AddDays(-2), now.AddDays(-1), now.AddHours(-1), now.AddMinutes(-1), now.AddDays(1));
        var db = scope.Services.GetRequiredService<DropCommerceDbContext>();
        db.Add(drop);
        await db.SaveChangesAsync();

        return drop;
    }

    private static async Task PublishAsync(DropAppearanceTestScope scope, DropEvent drop)
    {
        drop.Update(drop.EnterpriseId, null, drop.Name, drop.Slug, drop.Description, drop.CoverImageUrl, drop.BannerImageUrl,
            5, 10, 0, 0, 10, false, true, drop.RegistrationStartsAt, drop.RegistrationEndsAt, drop.QueueOpensAt, drop.DropStartsAt, drop.DropEndsAt);
        await scope.Services.GetRequiredService<DropCommerceDbContext>().SaveChangesAsync();
    }

    private sealed class PurchaseClock : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = new(DateTime.UtcNow.Ticks / 10 * 10, DateTimeKind.Utc);

        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(UtcNow);
        }

        public void Advance(TimeSpan duration)
        {
            UtcNow += duration;
        }
    }

    private sealed class TestDropGateway : IDropSaleGateway
    {
        public int Creates { get; private set; }
        public int Recoveries { get; private set; }
        public bool LoseFirstResponse { get; set; }
        public string Status { get; set; } = "pending";

        public Task<PaymentProviderContext> ContextAsync(long enterpriseId, CancellationToken ct)
        {
            return Task.FromResult(new PaymentProviderContext("mercadopago", IntegrationEnvironment.Sandbox, "test-seller", null));
        }

        public Task<PaymentSession?> CreateAsync(PaymentRequest request, bool recoverOnly, CancellationToken ct)
        {
            if (!recoverOnly)
                Creates++;
            else
                Recoveries++;

            if (LoseFirstResponse && !recoverOnly)
            {
                LoseFirstResponse = false;

                throw new HttpRequestException("Resposta perdida após criar a preferência fictícia.");
            }

            return Task.FromResult<PaymentSession?>(new("test-preference", "https://sandbox.mercadopago.com.br/checkout/test",
                null, request.ExpiresAt, "test-preference"));
        }

        public Task<DropSaleGatewayResult> ReadAsync(PaymentRequest request, CancellationToken ct)
        {
            return Task.FromResult(new DropSaleGatewayResult(Status, Status == "approved" ? "test-payment" : null));
        }
    }
}
