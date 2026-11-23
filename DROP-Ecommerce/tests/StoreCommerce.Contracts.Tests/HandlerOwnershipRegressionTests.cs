using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class HandlerOwnershipRegressionTests(ContractsApiFixture fixture)
{
    [PostgresFact]
    public async Task Cart_operations_and_address_changes_preserve_selection_and_default_address()
    {
        var (owner, productId, _) = await ProductAsync();
        using var buyer = await BuyerAsync(owner.EnterpriseId);
        var first = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/me/addresses", Address("Primeiro", false)));
        var firstId = first.EnumerateArray().Single().GetProperty("id").GetInt64();
        Assert.True(first.EnumerateArray().Single().GetProperty("isDefault").GetBoolean());

        var second = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/me/addresses", Address("Segundo", false)));
        var secondId = second.EnumerateArray().Single(Item => Item.GetProperty("id").GetInt64() != firstId).GetProperty("id").GetInt64();
        var defaults = await ContentAsync(await buyer.PutAsync($"/api/storefront/me/addresses/{secondId}/default", null));
        Assert.Equal(secondId, defaults.EnumerateArray().Single(Item => Item.GetProperty("isDefault").GetBoolean()).GetProperty("id").GetInt64());

        await ContentAsync(await buyer.PutAsJsonAsync($"/api/storefront/me/addresses/{firstId}", Address("Atualizado", false)));
        var remaining = await ContentAsync(await buyer.DeleteAsync($"/api/storefront/me/addresses/{secondId}"));
        var remainingAddress = remaining.EnumerateArray().Single();
        Assert.Equal(firstId, remainingAddress.GetProperty("id").GetInt64());
        Assert.Equal("Atualizado", remainingAddress.GetProperty("label").GetString());
        Assert.True(remainingAddress.GetProperty("isDefault").GetBoolean());

        var cart = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId, quantity = 1 }));
        var itemId = cart.GetProperty("items").EnumerateArray().Single().GetProperty("id").GetInt64();
        cart = await ContentAsync(await buyer.PutAsJsonAsync($"/api/storefront/cart/items/{itemId}", new { quantity = 2 }));
        Assert.Equal(2, cart.GetProperty("items").EnumerateArray().Single().GetProperty("quantity").GetInt32());

        cart = await ContentAsync(await buyer.PutAsJsonAsync("/api/storefront/cart/selection", new { selected = false }));
        Assert.False(cart.GetProperty("items").EnumerateArray().Single().GetProperty("isSelected").GetBoolean());
        cart = await ContentAsync(await buyer.PutAsJsonAsync("/api/storefront/cart/selection", new { selected = true }));
        Assert.True(cart.GetProperty("items").EnumerateArray().Single().GetProperty("isSelected").GetBoolean());

        cart = await ContentAsync(await buyer.DeleteAsync($"/api/storefront/cart/items/{itemId}"));
        Assert.Empty(cart.GetProperty("items").EnumerateArray());
        await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId, quantity = 1 }));
        cart = await ContentAsync(await buyer.DeleteAsync("/api/storefront/cart"));
        Assert.Empty(cart.GetProperty("items").EnumerateArray());
        var savedAddresses = await ContentAsync(await buyer.GetAsync("/api/storefront/me/addresses"));
        Assert.Single(savedAddresses.EnumerateArray());
    }

    [PostgresFact]
    public async Task Cart_waiting_for_stock_survives_replenishment_without_selecting_the_item()
    {
        var (owner, productId, variantId) = await ProductAsync();
        using var buyer = await BuyerAsync(owner.EnterpriseId);
        var cart = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId, quantity = 1 }));
        var itemId = cart.GetProperty("items").EnumerateArray().Single().GetProperty("id").GetInt64();
        await AdjustAsync(owner, variantId, -8);
        cart = await ContentAsync(await buyer.PutAsync($"/api/storefront/cart/items/{itemId}/await-stock", null));
        Assert.True(cart.GetProperty("items").EnumerateArray().Single().GetProperty("isAwaitingStock").GetBoolean());

        await AdjustAsync(owner, variantId, 8);
        cart = await ContentAsync(await buyer.GetAsync("/api/storefront/cart"));
        var item = cart.GetProperty("items").EnumerateArray().Single();
        Assert.True(item.GetProperty("backInStock").GetBoolean());
        Assert.False(item.GetProperty("isSelected").GetBoolean());
    }

    [PostgresFact]
    public async Task Repeated_checkout_and_customer_cancellation_do_not_duplicate_orders_or_release_stock_twice()
    {
        var (owner, productId, variantId) = await ProductAsync();
        using var buyer = await BuyerAsync(owner.EnterpriseId);
        await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId, quantity = 2 }));
        var request = Checkout();
        var order = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/checkout", request));
        var orderId = order.GetProperty("id").GetInt64();
        var repeated = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/checkout", request));
        Assert.Equal(orderId, repeated.GetProperty("id").GetInt64());
        Assert.Equal((8, 2), await StockAsync(variantId));

        var cancelled = await ContentAsync(await buyer.PostAsJsonAsync($"/api/storefront/orders/{orderId}/cancel", new { reason = "Teste de cancelamento" }));
        Assert.Equal(OrderStatus.Cancelled.Id, cancelled.GetProperty("statusId").GetInt64());
        await ContentAsync(await buyer.PostAsJsonAsync($"/api/storefront/orders/{orderId}/cancel", new { reason = "Repetição" }));
        Assert.Equal((8, 0), await StockAsync(variantId));

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.Equal(1, await db.Orders.IgnoreQueryFilters().CountAsync(Order => Order.EnterpriseId == owner.EnterpriseId));
    }

    [PostgresFact]
    public async Task Payment_confirmation_processing_shipping_and_delivery_keep_stock_and_history_consistent()
    {
        var (owner, productId, variantId) = await ProductAsync();
        using var buyer = await BuyerAsync(owner.EnterpriseId);
        await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId, quantity = 2 }));
        var order = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/checkout", Checkout()));
        var orderId = order.GetProperty("id").GetInt64();
        using var backoffice = fixture.Client(owner.Token);

        await ContentAsync(await backoffice.PostAsJsonAsync($"/api/orders/{orderId}/confirm-payment", new { transactionMethodId = TransactionMethod.Pix.Id }));
        await ContentAsync(await backoffice.PostAsJsonAsync($"/api/orders/{orderId}/confirm-payment", new { transactionMethodId = TransactionMethod.Pix.Id }));
        Assert.Equal((6, 0), await StockAsync(variantId));
        await ContentAsync(await backoffice.PostAsync($"/api/orders/{orderId}/start-processing", null));
        await ContentAsync(await backoffice.PostAsJsonAsync($"/api/orders/{orderId}/ship", new
        {
            shipmentTypeId = ShipmentType.Pickup.Id,
            estimatedDelivery = DateTime.UtcNow.AddDays(2)
        }));
        var delivered = await ContentAsync(await backoffice.PostAsync($"/api/orders/{orderId}/deliver", null));
        Assert.Equal(OrderStatus.Delivered.Id, delivered.GetProperty("statusId").GetInt64());
        Assert.Equal((6, 0), await StockAsync(variantId));
        Assert.NotEmpty(delivered.GetProperty("history").EnumerateArray());
    }

    [PostgresFact]
    public async Task Zero_total_checkout_confirms_once_without_creating_a_provider_attempt()
    {
        var (owner, productId, variantId) = await ProductAsync();
        var code = "FREE-" + Guid.NewGuid().ToString("N");
        using (var backoffice = fixture.Client(owner.Token))
        {
            await ContentAsync(await backoffice.PostAsJsonAsync("/api/coupons/add", new
            {
                code,
                typeId = CouponType.Percentage.Id,
                discountValue = 100m,
                maxDiscountCap = 100m,
                maxUses = 1,
                isActive = true,
                isSingleUse = true,
                startsAt = DateTime.UtcNow.AddDays(-1),
                expiresAt = DateTime.UtcNow.AddDays(1),
                conditions = Array.Empty<object>()
            }));
        }

        using var buyer = await BuyerAsync(owner.EnterpriseId);
        await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId, quantity = 1 }));
        var request = Checkout(code);
        var order = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/checkout", request));
        var repeated = await ContentAsync(await buyer.PostAsJsonAsync("/api/storefront/checkout", request));
        Assert.Equal(OrderStatus.Confirmed.Id, order.GetProperty("statusId").GetInt64());
        Assert.Equal(order.GetProperty("id").GetInt64(), repeated.GetProperty("id").GetInt64());
        Assert.Equal((7, 0), await StockAsync(variantId));

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        Assert.False(await db.PaymentAttempts.IgnoreQueryFilters().AnyAsync(Attempt => Attempt.EnterpriseId == owner.EnterpriseId));
        Assert.Equal(1, (await db.Coupons.IgnoreQueryFilters().SingleAsync(Coupon => Coupon.EnterpriseId == owner.EnterpriseId)).UsedCount);
    }

    [Fact]
    public void Application_features_have_no_business_service_types_or_handler_dependencies()
    {
        var assembly = typeof(StoreCommerce.Application.DependencyInjection).Assembly;
        var services = assembly.GetTypes().Where(Type => Type.IsClass &&
            Type.Namespace?.StartsWith("StoreCommerce.Application.Integrations", StringComparison.Ordinal) != true &&
            Type.Name.EndsWith("Service", StringComparison.Ordinal));
        Assert.Empty(services);

        foreach (var handler in assembly.GetTypes().Where(Type => !Type.IsAbstract && Type.GetInterfaces().Any(Interface =>
            Interface.IsGenericType && Interface.GetGenericTypeDefinition() is var Definition &&
            (Definition == typeof(IRequestHandler<>) || Definition == typeof(IRequestHandler<,>)))))
        {
            Assert.DoesNotContain(handler.GetConstructors().SelectMany(Constructor => Constructor.GetParameters()), Parameter =>
                Parameter.ParameterType.Namespace?.StartsWith("StoreCommerce.Application", StringComparison.Ordinal) == true &&
                Parameter.ParameterType.Name.EndsWith("Service", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Shared_order_and_reservation_writers_do_not_own_transactions_or_dispatch_requests()
    {
        var assembly = typeof(StoreCommerce.Application.DependencyInjection).Assembly;
        var writers = assembly.GetTypes().Where(Type => Type.Name.EndsWith("Writer", StringComparison.Ordinal) &&
            (Type.Name.StartsWith("Order", StringComparison.Ordinal) || Type.Name.StartsWith("StockReservation", StringComparison.Ordinal))).ToArray();
        Assert.NotEmpty(writers);

        foreach (var writer in writers)
        {
            Assert.DoesNotContain(writer.GetConstructors().SelectMany(Constructor => Constructor.GetParameters()), Parameter =>
                Parameter.ParameterType.Name is "IUnitOfWork" or "IIdentityUnitOfWork" or "ISender" or "IMediator");
        }
    }

    private async Task<(StoreActor Owner, long ProductId, long VariantId)> ProductAsync()
    {
        var owner = await fixture.CreateStoreAsync(activeProducts: 1);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.EnterpriseId == owner.EnterpriseId);
        await AdjustAsync(owner, variant.Id, 8);

        return (owner, variant.ProductId, variant.Id);
    }

    private async Task<HttpClient> BuyerAsync(long enterpriseId)
    {
        var customer = await fixture.CreateCustomerAsync(enterpriseId);
        var client = fixture.Client(customer.Token);
        client.BaseAddress = customer.Address;

        return client;
    }

    private async Task AdjustAsync(StoreActor owner, long variantId, int quantity)
    {
        using var client = fixture.Client(owner.Token);
        await ContentAsync(await client.PutAsJsonAsync($"/api/inventory/variants/{variantId}/adjust", new
        {
            quantityChange = quantity,
            reason = "Teste local de responsabilidade dos handlers",
            idempotencyKey = Guid.NewGuid().ToString("N")
        }));
    }

    private async Task<(int Quantity, int Reserved)> StockAsync(long variantId)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().AsNoTracking().SingleAsync(Variant => Variant.Id == variantId);

        return (variant.StockQuantity, variant.ReservedQuantity);
    }

    private static object Address(string label, bool isDefault)
    {
        return new { label, recipientName = "Comprador", addressLine = "Rua Teste, 10", city = "São Paulo", stateId = 1, zipCode = "01001000", isDefault };
    }

    private static object Checkout(string? couponCode = null)
    {
        return new { idempotencyKey = Guid.NewGuid().ToString("N"), paymentMethodId = TransactionMethod.Pix.Id, useProfileAddress = true, couponCode };
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
}
