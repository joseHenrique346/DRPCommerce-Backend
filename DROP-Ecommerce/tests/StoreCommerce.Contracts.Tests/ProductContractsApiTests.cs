using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Providers;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class ProductContractsApiTests(ContractsApiFixture fixture)
{
    private const string Reactivation = "/api/products/contract-reactivation";

    [PostgresFact]
    public async Task Migration_blocks_only_newest_excess_and_keeps_archived_products_and_snapshots()
    {
        var products = await ProductsAsync(fixture.LegacyEnterpriseIds[0]);
        Assert.Equal(54, products.Count);
        Assert.Equal(2, products.Count(Product => Product.IsContractBlocked));
        Assert.All(products.Take(50), Product => Assert.False(Product.IsContractBlocked));
        Assert.All(products.Skip(50).Take(2), Product => Assert.True(Product.IsContractBlocked && Product.IsActive));
        Assert.All(products.Skip(52), Product => Assert.False(Product.IsContractBlocked || Product.IsActive));

        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var events = await db.StoreActivities.IgnoreQueryFilters().Where(Activity =>
            Activity.EnterpriseId == fixture.LegacyEnterpriseIds[0]).ToListAsync();
        Assert.Equal(2, events.Count);
        Assert.All(events, Activity => Assert.Equal("system", Activity.ActorType));
        Assert.All(events, Activity => Assert.StartsWith("LEGACY-", Activity.SKU));
    }

    [PostgresFact]
    public async Task Archived_products_count_and_a_batch_cannot_partially_take_the_last_slot()
    {
        var actor = await fixture.CreateStoreAsync(activeProducts: 10, inactiveProducts: 39);
        var category = (await ProductsAsync(actor.EnterpriseId))[0].CategoryId;
        using var client = fixture.Client(actor.Token);
        using var denied = await client.PostAsJsonAsync("/api/products/add-range",
            new { commands = new[] { NewProduct(category), NewProduct(category) } });
        Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);
        Assert.Equal(49, (await ProductsAsync(actor.EnterpriseId)).Count);
        using var accepted = await client.PostAsJsonAsync("/api/products/add", NewProduct(category));
        accepted.EnsureSuccessStatusCode();
        Assert.Equal(50, (await ProductsAsync(actor.EnterpriseId)).Count);
        using var full = await client.PostAsJsonAsync("/api/products/add", NewProduct(category));
        Assert.Equal(HttpStatusCode.Conflict, full.StatusCode);
    }

    [PostgresFact]
    public async Task Concurrent_creations_cannot_oversubscribe_the_last_slot()
    {
        var actor = await fixture.CreateStoreAsync(inactiveProducts: 49);
        var category = (await ProductsAsync(actor.EnterpriseId))[0].CategoryId;
        using var client = fixture.Client(actor.Token);
        var responses = await Task.WhenAll(Enumerable.Range(0, 4)
            .Select(Index => client.PostAsJsonAsync("/api/products/add", NewProduct(category))));

        try
        {
            Assert.Single(responses.Where(Response => Response.IsSuccessStatusCode));
            Assert.Equal(3, responses.Count(Response => Response.StatusCode == HttpStatusCode.Conflict));
            Assert.Equal(50, (await ProductsAsync(actor.EnterpriseId)).Count);
        }
        finally
        {
            foreach (var response in responses)
                response.Dispose();
        }
    }

    [PostgresFact]
    public async Task Downgrade_keeps_oldest_publications_and_upgrade_requires_explicit_reactivation()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 53, inactiveProducts: 2);
        var original = await ProductsAsync(actor.EnterpriseId);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        var downgraded = await ProductsAsync(actor.EnterpriseId);
        Assert.Equal(55, downgraded.Count);
        Assert.Equal(original.Take(50).Select(Product => Product.Id),
            downgraded.Where(Product => Product.IsActive && !Product.IsContractBlocked).Select(Product => Product.Id));
        Assert.Equal(3, downgraded.Count(Product => Product.IsContractBlocked));
        await fixture.ChangeContractAsync(actor.EnterpriseId, 2, null);
        Assert.Equal(3, (await ProductsAsync(actor.EnterpriseId)).Count(Product => Product.IsContractBlocked));

        using var client = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var preview = await client.PostAsJsonAsync(Reactivation + "/preview", new { expectedContractRevision = 4, mode = "oldest" });
        preview.EnsureSuccessStatusCode();
        using var previewJson = await JsonAsync(preview);
        Assert.Equal(3, previewJson.RootElement.GetProperty("content").GetProperty("selectedProducts").GetArrayLength());
        Assert.Equal(3, (await ProductsAsync(actor.EnterpriseId)).Count(Product => Product.IsContractBlocked));
        using var confirmed = await client.PostAsJsonAsync(Reactivation, new { expectedContractRevision = 4, mode = "oldest" });
        confirmed.EnsureSuccessStatusCode();
        Assert.DoesNotContain(await ProductsAsync(actor.EnterpriseId), Product => Product.IsContractBlocked);
        Assert.Equal(2, (await ProductsAsync(actor.EnterpriseId)).Count(Product => !Product.IsActive));
    }

    [PostgresFact]
    public async Task Archiving_opens_a_publication_slot_but_does_not_free_creation_quota()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 52);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        var products = await ProductsAsync(actor.EnterpriseId);
        using var client = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var noSlot = await client.PostAsJsonAsync(Reactivation, new { expectedContractRevision = 3, mode = "oldest" });
        Assert.Equal(HttpStatusCode.Conflict, noSlot.StatusCode);
        using var archived = await client.PutAsJsonAsync($"/api/products/{products[0].Id}/active", new { isActive = false });
        archived.EnsureSuccessStatusCode();
        var selectedId = products[^1].Id;
        using var restored = await client.PostAsJsonAsync(Reactivation,
            new { expectedContractRevision = 3, mode = "selected", productIds = new[] { selectedId } });
        restored.EnsureSuccessStatusCode();
        using var creation = await client.PostAsJsonAsync("/api/products/add", NewProduct(products[0].CategoryId));
        Assert.Equal(HttpStatusCode.Conflict, creation.StatusCode);
        var current = await ProductsAsync(actor.EnterpriseId);
        Assert.False(current.Single(Product => Product.Id == selectedId).IsContractBlocked);
        Assert.True(current[^2].IsContractBlocked);
    }

    [PostgresFact]
    public async Task Reactivation_rejects_stale_revision_foreign_ids_and_partial_selection()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 52);
        var foreign = await fixture.CreateStoreAsync(activeProducts: 1);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var client = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var stale = await client.PostAsJsonAsync(Reactivation, new { expectedContractRevision = 1, mode = "oldest" });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var other = await client.PostAsJsonAsync(Reactivation,
            new { expectedContractRevision = 3, mode = "selected", productIds = new[] { (await ProductsAsync(foreign.EnterpriseId))[0].Id } });
        Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
        var products = await ProductsAsync(actor.EnterpriseId);
        using var archived = await client.PutAsJsonAsync($"/api/products/{products[0].Id}/active", new { isActive = false });
        archived.EnsureSuccessStatusCode();
        using var tooMany = await client.PostAsJsonAsync(Reactivation,
            new { expectedContractRevision = 3, mode = "selected", productIds = products.TakeLast(2).Select(Product => Product.Id) });
        Assert.Equal(HttpStatusCode.Conflict, tooMany.StatusCode);
        Assert.Equal(2, (await ProductsAsync(actor.EnterpriseId)).Count(Product => Product.IsContractBlocked));
    }

    [PostgresFact]
    public async Task Impact_preview_matches_downgrade_without_changing_contract_or_products()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 53, inactiveProducts: 2);
        using var client = fixture.Client(actor.Token);
        using var preview = await client.PostAsJsonAsync("/api/enterprises/current/contract/product-impact",
            new { planId = 1, expectedContractRevision = 2, catalogVersion = 1 });
        preview.EnsureSuccessStatusCode();
        using var json = await JsonAsync(preview);
        Assert.Equal(3, json.RootElement.GetProperty("productsToBlock").GetInt32());
        Assert.Equal(5, json.RootElement.GetProperty("excessTotalProducts").GetInt32());
        Assert.DoesNotContain(await ProductsAsync(actor.EnterpriseId), Product => Product.IsContractBlocked);
    }

    [PostgresFact]
    public async Task Blocked_products_are_readable_but_cannot_be_edited_or_archived()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        var blocked = (await ProductsAsync(actor.EnterpriseId))[^1];
        using var client = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var readable = await client.GetAsync($"/api/products/get-by-id/{blocked.Id}");
        readable.EnsureSuccessStatusCode();
        using var archived = await client.PutAsJsonAsync($"/api/products/{blocked.Id}/active", new { isActive = false });
        using var edited = await client.PutAsJsonAsync($"/api/products/{blocked.Id}/pricing", new { price = 12m, costPrice = 1m });
        using var variants = await client.PutAsync($"/api/products/{blocked.Id}/variants/enable", null);
        Assert.Equal(HttpStatusCode.Conflict, archived.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, edited.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, variants.StatusCode);
    }

    [PostgresFact]
    public async Task Blocked_product_deletion_keeps_available_stock_and_immutable_history()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        var product = (await ProductsAsync(actor.EnterpriseId))[^1];
        await StockAsync(actor.EnterpriseId, product.Id, 7);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var client = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var deleted = await client.DeleteAsync($"/api/products/delete/{product.Id}");
        deleted.EnsureSuccessStatusCode();
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var persisted = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.ProductId == product.Id);
        Assert.True(persisted.IsDeleted);
        Assert.Equal(7, persisted.StockQuantity);
        using var history = await client.GetAsync($"/api/store-activity?productId={product.Id}");
        history.EnsureSuccessStatusCode();
        var body = await history.Content.ReadAsStringAsync();
        Assert.Contains("product_deleted", body);
        Assert.Contains(product.Name, body);
        Assert.Contains("\"actorType\":\"user\"", body);
    }

    [PostgresFact]
    public async Task Active_reservations_prevent_blocked_deletion_and_can_still_be_released()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        var product = (await ProductsAsync(actor.EnterpriseId))[^1];
        await StockAsync(actor.EnterpriseId, product.Id, 7, reserved: 2);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var client = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var denied = await client.DeleteAsync($"/api/products/delete/{product.Id}");
        Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);

        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        using var inventory = services.GetRequiredService<InventoryWriteScope>().Begin(actor.EnterpriseId);
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.ProductId == product.Id);
        variant.ReleaseStock(2);
        await db.SaveChangesAsync();
        Assert.Equal(7, variant.StockQuantity);
        Assert.Equal(0, variant.ReservedQuantity);
    }

    [PostgresFact]
    public async Task Persistence_refuses_forged_unblock_and_history_modification_even_in_identity_scope()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var product = await db.Products.IgnoreQueryFilters().SingleAsync(Product =>
            Product.EnterpriseId == actor.EnterpriseId && Product.IsContractBlocked);
        product.UnblockForContract();
        var blocked = await Assert.ThrowsAsync<SecurityFailure>(() => db.SaveChangesAsync());
        Assert.Equal("ProductContract.UseReactivation", blocked.Code);
        db.ChangeTracker.Clear();
        var activity = await db.StoreActivities.IgnoreQueryFilters().FirstAsync(Activity => Activity.EnterpriseId == actor.EnterpriseId);
        db.StoreActivities.Remove(activity);
        var immutable = await Assert.ThrowsAsync<SecurityFailure>(() => db.SaveChangesAsync());
        Assert.Equal("StoreActivity.Immutable", immutable.Code);
    }

    [PostgresFact]
    public async Task History_requires_its_own_permission_and_cannot_cross_tenants()
    {
        var actor = await fixture.CreateStoreAsync(activeProducts: 1);
        var foreign = await fixture.CreateStoreAsync(activeProducts: 1);
        var staff = await fixture.CreateStaffAsync(actor.EnterpriseId);
        using var employee = fixture.Client(staff.Token);
        using var forbidden = await employee.GetAsync("/api/store-activity");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        using var owner = fixture.Client(actor.Token);
        using var otherProduct = await owner.GetAsync($"/api/store-activity?productId={(await ProductsAsync(foreign.EnterpriseId))[0].Id}");
        otherProduct.EnsureSuccessStatusCode();
        using var json = await JsonAsync(otherProduct);
        Assert.Equal(0, json.RootElement.GetProperty("content").GetProperty("totalCount").GetInt32());
        using var invalid = await owner.GetAsync("/api/store-activity?pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var mutation = await owner.PostAsJsonAsync("/api/store-activity", new { eventType = "forged" });
        Assert.Equal(HttpStatusCode.MethodNotAllowed, mutation.StatusCode);
    }

    [PostgresFact]
    public async Task Store_module_loss_blocks_archived_products_and_module_return_does_not_publish_them()
    {
        var actor = await fixture.CreateStoreAsync(activeProducts: 2, inactiveProducts: 1);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 5, null);
        Assert.All(await ProductsAsync(actor.EnterpriseId), Product => Assert.True(Product.IsContractBlocked));
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        var products = await ProductsAsync(actor.EnterpriseId);
        Assert.All(products.Where(Product => Product.IsActive), Product => Assert.True(Product.IsContractBlocked));
        Assert.False(products.Single(Product => !Product.IsActive).IsContractBlocked);
    }

    [PostgresFact]
    public async Task Published_appearance_removes_unavailable_product_references_without_editing_saved_settings()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        var products = await ProductsAsync(actor.EnterpriseId);
        var settings = JsonSerializer.Serialize(new { featuredProducts = new[] { products[0].Id, products[^1].Id }, hero = products[^1].Id });
        const string schema = """
            {"type":"object","properties":{"featuredProducts":{"type":"array","items":{"type":"integer","x-resource":"product"}},"hero":{"type":"integer","x-resource":"product"}}}
            """;
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var resources = new StoreAppearanceResources(db);
        var filtered = await resources.FilterPublishedSettingsAsync(actor.EnterpriseId, schema, settings, default);
        using var json = JsonDocument.Parse(filtered);
        Assert.Single(json.RootElement.GetProperty("featuredProducts").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("hero").ValueKind);
        using var original = JsonDocument.Parse(settings);
        Assert.Equal(2, original.RootElement.GetProperty("featuredProducts").GetArrayLength());
    }

    [PostgresFact]
    public async Task Public_listing_search_slug_and_existing_cart_do_not_expose_blocked_product()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        var product = (await ProductsAsync(actor.EnterpriseId))[^1];
        await StockAsync(actor.EnterpriseId, product.Id, 4);
        var customer = await fixture.CreateCustomerAsync(actor.EnterpriseId);
        using var client = fixture.Client(customer.Token);
        client.BaseAddress = customer.Address;
        using var cart = await client.PostAsJsonAsync("/api/storefront/cart/items", new { productId = product.Id, quantity = 1 });
        cart.EnsureSuccessStatusCode();
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);

        using var publicClient = fixture.Client();
        publicClient.BaseAddress = customer.Address;
        using var listing = await publicClient.GetAsync("/api/storefront/products?pageSize=100");
        listing.EnsureSuccessStatusCode();
        Assert.DoesNotContain(product.Slug, await listing.Content.ReadAsStringAsync());
        using var search = await publicClient.GetAsync("/api/storefront/products/search?term=" + Uri.EscapeDataString(product.SKU));
        search.EnsureSuccessStatusCode();
        using var searchJson = await JsonAsync(search);
        Assert.Equal(0, searchJson.RootElement.GetProperty("content").GetProperty("totalCount").GetInt32());
        using var slug = await publicClient.GetAsync("/api/storefront/products/by-slug/" + product.Slug);
        Assert.Equal(HttpStatusCode.NotFound, slug.StatusCode);
        using var maskedCart = await client.GetAsync("/api/storefront/cart");
        maskedCart.EnsureSuccessStatusCode();
        var cartBody = await maskedCart.Content.ReadAsStringAsync();
        Assert.DoesNotContain(product.Name, cartBody);
        Assert.DoesNotContain(product.SKU, cartBody);
        using var maskedJson = JsonDocument.Parse(cartBody);
        Assert.False(maskedJson.RootElement.GetProperty("content").GetProperty("summary").GetProperty("canCheckout").GetBoolean());
        using var newItem = await client.PostAsJsonAsync("/api/storefront/cart/items", new { productId = product.Id, quantity = 1 });
        Assert.Equal(HttpStatusCode.NotFound, newItem.StatusCode);
        using var checkout = await client.PostAsJsonAsync("/api/storefront/checkout", Checkout());
        Assert.Equal(HttpStatusCode.Conflict, checkout.StatusCode);
    }

    [PostgresFact]
    public async Task Previously_created_order_can_be_paid_after_block_and_preserves_buyer_and_item_snapshots()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        var product = (await ProductsAsync(actor.EnterpriseId))[^1];
        await StockAsync(actor.EnterpriseId, product.Id, 4);
        var customer = await fixture.CreateCustomerAsync(actor.EnterpriseId);
        using var buyer = fixture.Client(customer.Token);
        buyer.BaseAddress = customer.Address;
        using var cart = await buyer.PostAsJsonAsync("/api/storefront/cart/items", new { productId = product.Id, quantity = 1 });
        cart.EnsureSuccessStatusCode();
        using var checkout = await buyer.PostAsJsonAsync("/api/storefront/checkout", Checkout());
        checkout.EnsureSuccessStatusCode();
        using var json = await JsonAsync(checkout);
        var orderId = json.RootElement.GetProperty("content").GetProperty("id").GetInt64();
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var owner = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var confirmed = await owner.PostAsJsonAsync($"/api/orders/{orderId}/confirm-payment",
            new { transactionMethodId = 2, note = "Teste local sem provedor externo" });
        confirmed.EnsureSuccessStatusCode();
        var response = await confirmed.Content.ReadAsStringAsync();
        Assert.Contains(product.Name, response);
        Assert.Contains("Comprador de teste", response);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.ProductId == product.Id);
        Assert.Equal(3, variant.StockQuantity);
        Assert.Equal(0, variant.ReservedQuantity);
        Assert.True((await ProductsAsync(actor.EnterpriseId))[^1].IsContractBlocked);
    }

    [PostgresFact]
    public async Task Blocked_inventory_refuses_new_adjustments_and_reservations_but_commits_existing_reservation()
    {
        var actor = await fixture.CreateStoreAsync(planId: 2, activeProducts: 51);
        var product = (await ProductsAsync(actor.EnterpriseId))[^1];
        await StockAsync(actor.EnterpriseId, product.Id, 4);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var variantId = await db.ProductVariants.IgnoreQueryFilters().Where(Variant => Variant.ProductId == product.Id)
            .Select(Variant => Variant.Id).SingleAsync();
        using var owner = fixture.Client(actor.Token);
        using var reserve = await owner.PostAsJsonAsync("/api/inventory/reservations",
            new { idempotencyKey = "before-block", items = new[] { new { productVariantId = variantId, quantity = 1 } } });
        reserve.EnsureSuccessStatusCode();
        using var reservation = await JsonAsync(reserve);
        var reservationId = reservation.RootElement.GetProperty("content").GetProperty("id").GetInt64();
        await fixture.ChangeContractAsync(actor.EnterpriseId, 1, null);
        using var refreshed = fixture.Client(await fixture.FreshTokenAsync(actor));
        using var adjustment = await refreshed.PutAsJsonAsync($"/api/inventory/variants/{variantId}/adjust",
            new { quantityChange = 1, reason = "Novo ajuste", idempotencyKey = "after-block" });
        Assert.Equal(HttpStatusCode.Conflict, adjustment.StatusCode);
        using var newReserve = await refreshed.PostAsJsonAsync("/api/inventory/reservations",
            new { idempotencyKey = "after-block", items = new[] { new { productVariantId = variantId, quantity = 1 } } });
        Assert.Equal(HttpStatusCode.Conflict, newReserve.StatusCode);
        using var commit = await refreshed.PutAsync($"/api/inventory/reservations/{reservationId}/commit", null);
        commit.EnsureSuccessStatusCode();
        var variant = await db.ProductVariants.IgnoreQueryFilters().AsNoTracking().SingleAsync(Variant => Variant.Id == variantId);
        Assert.Equal(3, variant.StockQuantity);
        Assert.Equal(0, variant.ReservedQuantity);
    }

    [PostgresFact]
    public async Task Additional_skus_do_not_consume_principal_product_quota()
    {
        var actor = await fixture.CreateStoreAsync(activeProducts: 50);
        var product = (await ProductsAsync(actor.EnterpriseId))[0];
        using var owner = fixture.Client(actor.Token);
        using var enabled = await owner.PutAsync($"/api/products/{product.Id}/variants/enable", null);
        enabled.EnsureSuccessStatusCode();
        using var scope = fixture.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var principal = await db.Products.IgnoreQueryFilters().SingleAsync(Product => Product.Id == product.Id);
        db.ProductVariants.Add(ProductVariant.Create(principal, "Azul", "BLUE-" + product.Id, "", new Dictionary<string, string> { ["cor"] = "azul" }));
        await db.SaveChangesAsync();
        Assert.Equal(50, (await ProductsAsync(actor.EnterpriseId)).Count);
        Assert.Equal(2, await db.ProductVariants.IgnoreQueryFilters().CountAsync(Variant => Variant.ProductId == product.Id));
    }

    [PostgresFact]
    public async Task Catalog_add_on_removal_blocks_excess_and_return_still_requires_reactivation()
    {
        var actor = await fixture.CreateStoreAsync(planId: 4, addOnId: 3, activeProducts: 102);
        await fixture.ChangeContractAsync(actor.EnterpriseId, 4, null);
        Assert.Equal(2, (await ProductsAsync(actor.EnterpriseId)).Count(Product => Product.IsContractBlocked));
        await fixture.ChangeContractAsync(actor.EnterpriseId, 4, 3);
        Assert.Equal(2, (await ProductsAsync(actor.EnterpriseId)).Count(Product => Product.IsContractBlocked));
    }

    [PostgresFact]
    public async Task History_permission_can_be_assigned_without_product_edit_permission()
    {
        var actor = await fixture.CreateStoreAsync(activeProducts: 1);
        var staff = await fixture.CreateStaffAsync(actor.EnterpriseId);
        using (var scope = fixture.Services.CreateScope())
        {
            using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
            var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
            var role = AccessRole.Create(actor.EnterpriseId, "Auditoria", "Consulta de histórico", true);
            db.AccessRoles.Add(role);
            await db.SaveChangesAsync();
            db.AccessRolePermissions.Add(AccessRolePermission.Create(actor.EnterpriseId, role.Id, 22));
            db.MembershipAccessRoles.Add(MembershipAccessRole.Create(actor.EnterpriseId, staff.MembershipId, role.Id));
            await db.SaveChangesAsync();
        }

        using var client = fixture.Client(await fixture.FreshTokenAsync(staff));
        using var history = await client.GetAsync("/api/store-activity?pageSize=1");
        history.EnsureSuccessStatusCode();
        using var reactivation = await client.PostAsJsonAsync(Reactivation, new { expectedContractRevision = 1, mode = "oldest" });
        Assert.Equal(HttpStatusCode.Forbidden, reactivation.StatusCode);
    }

    private static object Checkout()
    {
        return new { idempotencyKey = Guid.NewGuid().ToString("N"), paymentMethodId = 2, useProfileAddress = true };
    }

    private async Task<List<Product>> ProductsAsync(long tenant)
    {
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();

        return await db.Products.IgnoreQueryFilters().AsNoTracking().Where(Product =>
            Product.EnterpriseId == tenant && !Product.IsDeleted)
            .OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToListAsync();
    }

    private async Task StockAsync(long tenant, long productId, int stock, int reserved = 0)
    {
        using var scope = fixture.Services.CreateScope();
        var services = scope.ServiceProvider;
        using var identity = services.GetRequiredService<IIdentityOperationScope>().Begin();
        using var inventory = services.GetRequiredService<InventoryWriteScope>().Begin(tenant);
        var db = services.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.ProductId == productId);
        variant.AdjustStock(stock, false);

        if (reserved > 0)
            variant.ReserveStock(reserved, false);

        await db.SaveChangesAsync();
    }

    private static CreateProductCommand NewProduct(long category)
    {
        var suffix = Guid.NewGuid().ToString("N");

        return new CreateProductCommand(category, null, "Novo produto", "new-" + suffix, "", "NEW-" + suffix,
            "", 10m, 1m, 1m, 1m, 1m, 1m, "", "", true, false);
    }

    private static async Task<JsonDocument> JsonAsync(HttpResponseMessage response)
    {
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
