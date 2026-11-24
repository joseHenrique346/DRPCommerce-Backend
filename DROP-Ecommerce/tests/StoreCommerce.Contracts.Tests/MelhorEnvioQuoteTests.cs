using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class MelhorEnvioQuoteTests(MelhorEnvioQuoteFixture fixture) : IClassFixture<MelhorEnvioQuoteFixture>
{
    [PostgresFact]
    public async Task Quote_is_bound_to_cart_and_address_and_freight_is_charged_after_full_product_discount()
    {
        var (owner, productId, variantId) = await StoreWithShippingAsync(overrideDimensions: true);
        using var staff = fixture.Store.Client(owner.Token);
        var coupon = "FREE-" + Guid.NewGuid().ToString("N");
        await ContentAsync(await staff.PostAsJsonAsync("/api/coupons/add", new
        {
            code = coupon,
            typeId = CouponType.Percentage.Id,
            discountValue = 100m,
            maxDiscountCap = 100m,
            maxUses = 1,
            isActive = true,
            startsAt = DateTime.UtcNow.AddDays(-1),
            expiresAt = DateTime.UtcNow.AddDays(1),
            conditions = Array.Empty<object>()
        }));
        using var buyer = await BuyerAsync(owner.EnterpriseId, productId, 2, variantId);
        var shipping = await QuoteAsync(buyer);
        var input = fixture.Provider.LastRequest;
        Assert.Equal("01001000", input.GetProperty("from").GetProperty("postal_code").GetString());
        Assert.Equal("01001000", input.GetProperty("to").GetProperty("postal_code").GetString());
        var product = Assert.Single(input.GetProperty("products").EnumerateArray());
        Assert.Equal(1.5m, product.GetProperty("weight").GetDecimal());
        Assert.Equal(11m, product.GetProperty("height").GetDecimal());
        Assert.Equal(2, product.GetProperty("quantity").GetInt32());
        Assert.Equal(10m, product.GetProperty("insurance_value").GetDecimal());

        var request = Checkout(shipping, coupon);
        await ConflictAsync(await buyer.PostAsJsonAsync("checkout", request with { shippingQuoteToken = null }), "Checkout.ShippingQuoteRequired");
        await ConflictAsync(await buyer.PostAsJsonAsync("checkout", new
        {
            request.idempotencyKey,
            request.paymentMethodId,
            request.shippingQuoteToken,
            shippingAddressLine = "Outra rua",
            shippingCity = "São Paulo",
            shippingStateId = 1,
            shippingZipCode = "01001-000"
        }), "Checkout.ShippingQuoteChanged");

        var order = await ContentAsync(await buyer.PostAsJsonAsync("checkout", request));
        Assert.Equal(20m, order.GetProperty("discountAmount").GetDecimal());
        Assert.Equal(18.75m, order.GetProperty("shippingCost").GetDecimal());
        Assert.Equal(18.75m, order.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(18.75m, order.GetProperty("payment").GetProperty("amount").GetDecimal());
        Assert.Equal(2, order.GetProperty("shippingQuote").GetProperty("volumeCount").GetInt32());
        Assert.Equal(4, order.GetProperty("shippingQuote").GetProperty("deliveryDays").GetInt32());

        var repeated = await ContentAsync(await buyer.PostAsJsonAsync("checkout", request));
        Assert.Equal(order.GetProperty("id").GetInt64(), repeated.GetProperty("id").GetInt64());
        using var scope = fixture.Store.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var stored = await db.Orders.IgnoreQueryFilters().SingleAsync(Order => Order.EnterpriseId == owner.EnterpriseId);
        Assert.Equal(16m, stored.ShippingQuote!.Service.Volumes[1].LengthCm);
        Assert.Equal(18.75m, (await db.PaymentAttempts.IgnoreQueryFilters().SingleAsync(
            Attempt => Attempt.EnterpriseId == owner.EnterpriseId)).Amount);
    }

    [PostgresFact]
    public async Task Signed_quote_rejects_tampering_another_customer_changed_cart_and_expiry()
    {
        var (owner, productId, _) = await StoreWithShippingAsync();
        using var buyer = await BuyerAsync(owner.EnterpriseId, productId, 1);
        var token = await QuoteAsync(buyer);
        var tampered = (token[0] == 'a' ? "b" : "a") + token[1..];
        await ConflictAsync(await buyer.PostAsJsonAsync("checkout", Checkout(tampered)), "Checkout.InvalidShippingQuote");

        using var other = await BuyerAsync(owner.EnterpriseId, productId, 1);
        await ConflictAsync(await other.PostAsJsonAsync("checkout", Checkout(token)), "Checkout.InvalidShippingQuote");
        await ContentAsync(await buyer.PostAsJsonAsync("cart/items", new { productId, quantity = 1 }));
        await ConflictAsync(await buyer.PostAsJsonAsync("checkout", Checkout(token)), "Checkout.ShippingQuoteChanged");

        var current = await QuoteAsync(buyer);

        try
        {
            fixture.Clock.Offset = TimeSpan.FromMinutes(16);
            await ConflictAsync(await buyer.PostAsJsonAsync("checkout", Checkout(current)), "Checkout.ShippingQuoteExpired");
        }
        finally
        {
            fixture.Clock.Offset = TimeSpan.Zero;
        }

        using var scope = fixture.Store.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.False(await db.Orders.IgnoreQueryFilters().AnyAsync(Order => Order.EnterpriseId == owner.EnterpriseId));
        Assert.False(await db.PaymentAttempts.IgnoreQueryFilters().AnyAsync(Attempt => Attempt.EnterpriseId == owner.EnterpriseId));
    }

    [PostgresFact]
    public async Task Pickup_requires_store_opt_in_and_digital_orders_do_not_call_carriers()
    {
        var (owner, productId, _) = await StoreWithShippingAsync();
        using var buyer = await BuyerAsync(owner.EnterpriseId, productId, 1);
        var pickup = Checkout(null) with { useProfileAddress = false, pickupAtStore = true };
        var calls = fixture.Provider.Calls;
        await ConflictAsync(await buyer.PostAsJsonAsync("checkout", pickup), "Checkout.PickupUnavailable");

        using var staff = fixture.Store.Client(owner.Token);
        await ContentAsync(await staff.PutAsJsonAsync("/api/store-settings", new
        {
            currencyCode = "BRL",
            minOrderValue = 0,
            allowBackorder = false,
            allowPickup = true,
            pendingOrderExpirationMinutes = 60
        }));
        var pickedUp = await ContentAsync(await buyer.PostAsJsonAsync("checkout", pickup));
        Assert.True(pickedUp.GetProperty("pickupAtStore").GetBoolean());
        Assert.Equal(0, pickedUp.GetProperty("shippingCost").GetDecimal());

        using var scope = fixture.Store.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        await db.Products.IgnoreQueryFilters().Where(Product => Product.Id == productId)
            .ExecuteUpdateAsync(Setters => Setters.SetProperty(Product => Product.IsDigital, true));
        using var digitalBuyer = await BuyerAsync(owner.EnterpriseId, productId, 1);
        var digitalQuotes = await ContentAsync(await digitalBuyer.PostAsJsonAsync("checkout/shipping-quotes", new { useProfileAddress = true }));
        Assert.False(digitalQuotes.GetProperty("requiresQuote").GetBoolean());
        Assert.Empty(digitalQuotes.GetProperty("options").EnumerateArray());
        var digital = await ContentAsync(await digitalBuyer.PostAsJsonAsync("checkout", Checkout(null)));
        Assert.Equal(0, digital.GetProperty("shippingCost").GetDecimal());
        Assert.Equal(calls, fixture.Provider.Calls);
    }

    private async Task<(StoreActor Owner, long ProductId, long VariantId)> StoreWithShippingAsync(bool overrideDimensions = false)
    {
        var owner = await fixture.Store.CreateStoreAsync(activeProducts: 1);
        using var scope = fixture.Store.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.EnterpriseId == owner.EnterpriseId);
        var credentials = services.GetRequiredService<IProviderConnectionCredentials>();
        await credentials.SaveAuthorizedAsync(owner.EnterpriseId, IntegrationProviders.MelhorEnvio, IntegrationEnvironment.Sandbox,
            "1234", MelhorEnvioOptions.OAuthScopes,
            new ProviderTokens("test-shipping-access-token", "test-shipping-refresh-token", DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddDays(2)),
            owner.UserId, CancellationToken.None);
        using var staff = fixture.Store.Client(owner.Token);
        var variantId = variant.Id;

        if (overrideDimensions)
        {
            await ContentAsync(await staff.PutAsync($"/api/products/{variant.ProductId}/variants/enable", null));
            var created = await ContentAsync(await staff.PostAsJsonAsync($"/api/products/{variant.ProductId}/variants", new
            {
                details = new { name = "Azul", sku = $"BLUE-{variant.ProductId}", barCode = "", attributes = new { Cor = "Azul" } },
                dimensions = new { weight = 1.5m, height = 11m, width = 12m, length = 13m }
            }));
            variantId = created.GetProperty("id").GetInt64();
        }

        await ContentAsync(await staff.PutAsJsonAsync($"/api/inventory/variants/{variantId}/adjust", new
        {
            quantityChange = 12,
            reason = "Estoque de teste do frete",
            idempotencyKey = Guid.NewGuid().ToString("N")
        }));

        return (owner, variant.ProductId, variantId);
    }

    private async Task<HttpClient> BuyerAsync(long enterpriseId, long productId, int quantity, long? productVariantId = null)
    {
        var customer = await fixture.Store.CreateCustomerAsync(enterpriseId);
        var client = fixture.Store.Client(customer.Token);
        client.BaseAddress = customer.Address;
        await ContentAsync(await client.PostAsJsonAsync("cart/items", new { productId, productVariantId, quantity }));

        return client;
    }

    private static async Task<string> QuoteAsync(HttpClient buyer)
    {
        var response = await ContentAsync(await buyer.PostAsJsonAsync("checkout/shipping-quotes", new { useProfileAddress = true }));
        Assert.True(response.GetProperty("requiresQuote").GetBoolean());
        var option = Assert.Single(response.GetProperty("options").EnumerateArray());
        Assert.Equal(18.75m, option.GetProperty("price").GetDecimal());

        return option.GetProperty("quoteToken").GetString()!;
    }

    private static MelhorEnvioCheckoutBody Checkout(string? quote, string? coupon = null)
    {
        return new MelhorEnvioCheckoutBody(Guid.NewGuid().ToString("N"), TransactionMethod.Pix.Id, true, quote, coupon);
    }

    private static async Task ConflictAsync(HttpResponseMessage response, string code)
    {
        using (response)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.Conflict, $"HTTP {(int)response.StatusCode}: {body}");
            using var document = JsonDocument.Parse(body);
            Assert.Equal(code, document.RootElement.GetProperty("code").GetString());
        }
    }

    private static async Task<JsonElement> ContentAsync(HttpResponseMessage response)
    {
        using (response)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
            using var document = JsonDocument.Parse(body);

            return document.RootElement.TryGetProperty("content", out var content) ? content.Clone() : document.RootElement.Clone();
        }
    }

    private sealed record MelhorEnvioCheckoutBody(string idempotencyKey, long paymentMethodId, bool useProfileAddress,
        string? shippingQuoteToken, string? couponCode, bool pickupAtStore = false);
}
