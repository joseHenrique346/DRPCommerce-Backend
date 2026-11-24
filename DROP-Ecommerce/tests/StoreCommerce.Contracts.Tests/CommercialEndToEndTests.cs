using SharedCommerce.Application.Result;
using DropCommerce.Application.Features.Commands;
using MediatR;
using DropCommerce.Application.Contracts;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;
using Xunit.Abstractions;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
[Trait("Category", "TccDemo")]
public sealed class CommercialEndToEndTests(ITestOutputHelper output)
{
    [PostgresFact]
    public async Task Subscription_then_drop_purchase_uses_both_http_apis_and_confirms_stock_once()
    {
        await using var scenario = new CommercialScenario();
        await scenario.InitializeAsync();
        using (var free = scenario.Client(scenario.Owner.Token))
        {
            using var denied = await free.PostAsJsonAsync("/api/drop-catalog/products", Product());
            Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        }

        output.WriteLine("1. Gratuito não permite criar produtos DROP.");
        await scenario.ChooseAsync(3);
        using (var pending = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var usage = await CommercialScenario.JsonAsync(await pending.GetAsync("/api/drop-catalog/usage"));
            Assert.False(usage.GetProperty("allowsDrop").GetBoolean());
        }

        output.WriteLine("2. Checkout SaaS sem pagamento não concede o módulo.");
        await scenario.ApproveSubscriptionAsync();
        using (var stale = scenario.Client(scenario.Owner.Token))
        {
            using var denied = await stale.GetAsync("/api/drop-catalog/usage");
            Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        }

        var item = await CreateEventAsync(scenario);
        output.WriteLine("3. Pagamento SaaS confirmado libera DROP; sessão antiga é recusada.");
        var buyer = await scenario.Store.CreateCustomerAsync(scenario.Owner.EnterpriseId);
        using var client = scenario.Client(buyer.Token, buyer.Address);
        var detail = await CommercialScenario.JsonAsync(await client.GetAsync($"drop/events/{item.EventId}"));
        Assert.Equal(item.VariantId, detail.GetProperty("products")[0].GetProperty("variants")[0].GetProperty("id").GetInt64());
        var joined = await CommercialScenario.JsonAsync(await client.PostAsJsonAsync($"drop/events/{item.EventId}/queue", new { cpf = "52998224725" }));
        var session = joined.GetProperty("session");
        Assert.Equal(TimeSpan.FromMinutes(15), session.GetProperty("expiresAt").GetDateTime() - session.GetProperty("issuedAt").GetDateTime());
        var reserveBody = new { participationId = item.ParticipationId, sessionId = session.GetProperty("id").GetInt64(),
            idempotencyKey = "demo-purchase", lines = new[] { new { variantId = item.VariantId, quantity = 2 } } };
        var reservation = await CommercialScenario.JsonAsync(await client.PostAsJsonAsync($"drop/events/{item.EventId}/reservations", reserveBody));
        var reservationId = reservation.GetProperty("id").GetInt64();
        Assert.Equal(40m, reservation.GetProperty("totalAmount").GetDecimal());
        var purchase = await CommercialScenario.JsonAsync(await client.PostAsync($"drop/reservations/{reservationId}/checkout", null));
        var orderId = purchase.GetProperty("id").GetInt64();
        await CommercialScenario.JsonAsync(await client.PostAsync($"drop/reservations/{reservationId}/checkout", null));
        Assert.Single(scenario.Sales.Requests);
        Assert.Equal(1, purchase.GetProperty("dropOrderPaymentStatusId").GetInt64());
        output.WriteLine("4. Cliente entra na fila, reserva duas unidades e inicia um único checkout da loja.");
        scenario.Sales.Status = "approved";

        using (var scope = scenario.Drop.Services.CreateScope())
        {
            var checkout = scope.ServiceProvider.GetRequiredService<IMediator>();
            await checkout.SendRequiredAsync(new ReconcileDropPurchaseCommand(scenario.Owner.EnterpriseId, orderId), default);
            await checkout.SendRequiredAsync(new ReconcileDropPurchaseCommand(scenario.Owner.EnterpriseId, orderId), default);
        }

        var paid = await CommercialScenario.JsonAsync(await client.GetAsync($"drop/purchases/{orderId}"));
        Assert.Equal(2, paid.GetProperty("dropOrderPaymentStatusId").GetInt64());
        Assert.Equal("Comprador de teste", paid.GetProperty("buyerNameSnapshot").GetString());
        Assert.Equal(2, paid.GetProperty("items")[0].GetProperty("quantity").GetInt32());
        using (var scope = scenario.Drop.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DropCommerceDbContext>();
            Assert.Equal(3, await db.Set<DropCommerce.Domain.Entity.DropCatalogVariant>().IgnoreQueryFilters().SumAsync(Variant => Variant.QuantityOnHand));
            Assert.Equal(0, await db.Set<DropCommerce.Domain.Entity.DropCatalogVariant>().IgnoreQueryFilters().SumAsync(Variant => Variant.QuantityReserved));
        }

        using (var scope = scenario.Store.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            Assert.False(await db.Orders.IgnoreQueryFilters().AnyAsync(Order => Order.EnterpriseId == scenario.Owner.EnterpriseId));
            var intent = await db.Set<DropSalePaymentIntent>().IgnoreQueryFilters().SingleAsync();
            Assert.NotEqual((await scenario.SubscriptionAsync()).AccountId, intent.ExternalAccountId);
        }

        output.WriteLine("5. Confirmação repetida baixa estoque uma vez, preserva snapshots e separa conta SaaS da conta vendedora.");
        var secondBuyer = await scenario.Store.CreateCustomerAsync(scenario.Owner.EnterpriseId);
        using (var other = scenario.Client(secondBuyer.Token, secondBuyer.Address))
        {
            using var denied = await other.GetAsync($"drop/purchases/{orderId}");
            Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        }

        var secondStore = await scenario.Store.CreateStoreAsync(planId: 3);
        using (var other = scenario.Client(secondStore.Token))
        {
            using var denied = await other.PostAsJsonAsync($"/api/drop-purchases/{orderId}/fulfillment", new { statusId = 3 });
            Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
        }

        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            using var delivered = await owner.PostAsJsonAsync($"/api/drop-purchases/{orderId}/fulfillment", new { statusId = 5 });
            Assert.Equal(HttpStatusCode.NoContent, delivered.StatusCode);
        }

        output.WriteLine("6. Outro comprador/outra loja não acessa o pedido; gestor da loja confirma a entrega.");
    }

    [PostgresFact]
    public async Task Scheduled_downgrade_blocks_drop_at_boundary_and_upgrade_requires_explicit_reactivation()
    {
        await using var scenario = new CommercialScenario();
        await scenario.InitializeAsync();
        await scenario.ChooseAsync(3);
        await scenario.ApproveSubscriptionAsync();
        var item = await CreateEventAsync(scenario);
        var buyer = await scenario.Store.CreateCustomerAsync(scenario.Owner.EnterpriseId);
        var until = (await scenario.SubscriptionAsync()).PaidUntil!.Value;
        await scenario.ChooseAsync(2);
        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var usage = await CommercialScenario.JsonAsync(await owner.GetAsync("/api/drop-catalog/usage"));
            Assert.True(usage.GetProperty("allowsDrop").GetBoolean());
        }

        output.WriteLine("1. Downgrade agendado conserva DROP durante o período pago.");
        scenario.Clock.Set(until);
        scenario.Billing.AddInvoice((await scenario.SubscriptionAsync()).RemoteId!, until);
        await scenario.ReconcileBillingAsync();
        using (var storefront = scenario.Client(address: buyer.Address))
        {
            using var hidden = await storefront.GetAsync($"drop/events/{item.EventId}");
            Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        }

        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var products = await CommercialScenario.JsonAsync(await owner.GetAsync("/api/drop-catalog/products"));
            Assert.True(products[0].GetProperty("isContractBlocked").GetBoolean());
            Assert.Equal(5, products[0].GetProperty("variants")[0].GetProperty("quantityOnHand").GetInt32());
        }

        output.WriteLine("2. No vencimento, a oferta menor bloqueia a exposição DROP e preserva o catálogo/estoque.");
        await scenario.ChooseAsync(3);
        var upgrade = scenario.Billing.Checkouts.Single();
        using (var scope = scenario.Store.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            var invoice = await db.Set<PlatformBillingInvoice>().IgnoreQueryFilters().SingleAsync(Invoice => Invoice.IsUpgrade);
            scenario.Billing.PayUpgrade(upgrade.Key, invoice.Amount, scenario.Clock.GetUtcNow().UtcDateTime);
        }

        await scenario.ReconcileBillingAsync();
        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var usage = await CommercialScenario.JsonAsync(await owner.GetAsync("/api/drop-catalog/usage"));
            Assert.True(usage.GetProperty("allowsDrop").GetBoolean());
            Assert.Equal(1, usage.GetProperty("blockedParticipations").GetInt32());
            var selection = new { expectedContractRevision = usage.GetProperty("contractRevision").GetInt64(), mode = "selected",
                participationIds = new[] { item.ParticipationId } };
            await CommercialScenario.JsonAsync(await owner.PostAsJsonAsync("/api/drop-catalog/reactivation/preview", selection));
            await CommercialScenario.JsonAsync(await owner.PostAsJsonAsync("/api/drop-catalog/reactivation", selection));
        }

        using (var storefront = scenario.Client(address: buyer.Address))
            await CommercialScenario.JsonAsync(await storefront.GetAsync($"drop/events/{item.EventId}"));

        output.WriteLine("3. Upgrade pago recupera o direito; apenas reativação confirmada republica o evento.");
    }

    [PostgresFact]
    public async Task Grace_expiration_propagates_to_drop_even_during_provider_outage_and_recovery_is_explicit()
    {
        await using var scenario = new CommercialScenario();
        await scenario.InitializeAsync();
        await scenario.ChooseAsync(3);
        await scenario.ApproveSubscriptionAsync();
        var item = await CreateEventAsync(scenario);
        var buyer = await scenario.Store.CreateCustomerAsync(scenario.Owner.EnterpriseId);
        var subscription = await scenario.SubscriptionAsync();
        var until = subscription.PaidUntil!.Value;
        scenario.Billing.AddInvoice(subscription.RemoteId!, until, approved: false);
        scenario.Clock.Set(until.AddDays(6));
        await scenario.ReconcileBillingAsync();
        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var usage = await CommercialScenario.JsonAsync(await owner.GetAsync("/api/drop-catalog/usage"));
            Assert.True(usage.GetProperty("allowsDrop").GetBoolean());
        }

        output.WriteLine("1. Renovação rejeitada mantém direitos durante os sete dias fixos de carência.");
        scenario.Clock.Set(until.AddDays(7));
        scenario.Billing.Unavailable = true;
        using (var owner = scenario.Store.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            using var unavailable = await owner.PostAsync("/api/enterprises/current/billing/reconcile", null);
            Assert.False(unavailable.IsSuccessStatusCode);
        }

        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var usage = await CommercialScenario.JsonAsync(await owner.GetAsync("/api/drop-catalog/usage"));
            Assert.False(usage.GetProperty("allowsDrop").GetBoolean());
        }

        using (var storefront = scenario.Client(address: buyer.Address))
        {
            var list = await CommercialScenario.JsonAsync(await storefront.GetAsync("drop/events"));
            Assert.Equal(0, list.GetArrayLength());
        }

        output.WriteLine("2. A indisponibilidade do provedor não estende a carência; DROP deixa de aceitar exposição pública.");
        scenario.Billing.Unavailable = false;
        scenario.Billing.ApproveLastInvoice(subscription.RemoteId!, scenario.Clock.GetUtcNow().UtcDateTime);
        await scenario.ReconcileBillingAsync();
        using (var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner)))
        {
            var usage = await CommercialScenario.JsonAsync(await owner.GetAsync("/api/drop-catalog/usage"));
            Assert.True(usage.GetProperty("allowsDrop").GetBoolean());
            Assert.Equal(1, usage.GetProperty("blockedParticipations").GetInt32());
        }

        using (var storefront = scenario.Client(address: buyer.Address))
        {
            using var hidden = await storefront.GetAsync($"drop/events/{item.EventId}");
            Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        }

        Assert.Equal(until.AddMonths(1), (await scenario.SubscriptionAsync()).PaidUntil);
        output.WriteLine("3. Pagamento recuperado restaura o período original sem republicar produtos automaticamente.");
    }

    private static object Product()
    {
        return new { name = "Camiseta da demonstração", description = "Produto próprio DROP", variants = new[]
        {
            new { sku = "TCC-BLUE", options = new { color = "blue" }, price = 20m, quantity = 5 }
        } };
    }

    internal static async Task<ScenarioEvent> CreateEventAsync(CommercialScenario scenario, bool registrationOpen = false)
    {
        using var owner = scenario.Client(await scenario.Store.FreshTokenAsync(scenario.Owner));
        var product = await CommercialScenario.JsonAsync(await owner.PostAsJsonAsync("/api/drop-catalog/products", Product()));
        var now = scenario.Clock.GetUtcNow().UtcDateTime;
        var body = new { enterpriseId = scenario.Owner.EnterpriseId, productId = (long?)null, name = "DROP TCC", slug = "tcc-" + Guid.NewGuid().ToString("N"),
            description = "Demonstração integrada", coverImageUrl = "https://images.example.invalid/cover.png", bannerImageUrl = "https://images.example.invalid/banner.png",
            statusId = 1, totalUnitsAvailable = 5, unitsReserved = 0, unitsSold = 0, price = 20m, requiresRegistration = false, isPublic = false,
            registrationStartsAt = now.AddDays(-2), registrationEndsAt = registrationOpen ? now.AddDays(1) : now.AddDays(-1), queueOpensAt = now.AddHours(-1),
            dropStartsAt = now.AddMinutes(-1), dropEndsAt = now.AddMonths(3) };
        var added = await CommercialScenario.JsonAsync(await owner.PostAsJsonAsync("/api/drop-events/add", body));
        var eventId = added.GetProperty("content")[0].GetProperty("id").GetInt64();
        var participation = await CommercialScenario.JsonAsync(await owner.PostAsJsonAsync($"/api/drop-catalog/products/{product.GetProperty("id").GetInt64()}/events",
            new { dropEventId = eventId, maxPerCustomer = 3 }));
        var publish = JsonSerializer.SerializeToNode(body)!;
        publish["id"] = eventId;
        publish["statusId"] = 5;
        publish["isPublic"] = true;
        await CommercialScenario.JsonAsync(await owner.PutAsJsonAsync("/api/drop-events/update", publish));

        return new(eventId, product.GetProperty("id").GetInt64(), product.GetProperty("variants")[0].GetProperty("id").GetInt64(),
            participation.GetProperty("id").GetInt64());
    }

    internal sealed record ScenarioEvent(long EventId, long ProductId, long VariantId, long ParticipationId);
}
