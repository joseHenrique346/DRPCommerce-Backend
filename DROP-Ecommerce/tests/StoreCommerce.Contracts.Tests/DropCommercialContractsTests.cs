using SharedCommerce.Application.Interfaces.Appearance;
using StoreCommerce.Application.Interfaces.Identity;
using SharedCommerce.Application.Result;
using DropCommerce.Application.Features.Commands;
using MediatR;
using System.Net;
using System.Net.Http.Json;
using DropCommerce.Application.Contracts;
using DropCommerce.Domain.Entity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Application.Contracts.Drop;
using DropCommerce.Application.Appearance;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Infrastructure.Data.Context;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

[Collection("contracts-postgres")]
public sealed class DropCommercialContractsTests(ContractsApiFixture store)
{
    [PostgresFact]
    public async Task Own_catalog_accepts_null_store_reference_and_variants_share_one_creation_slot()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var catalog = drop.Services.GetRequiredService<IMediator>();
        var product = await catalog.SendRequiredAsync(new CreateDropCatalogProductCommand(Product("own")), default);
        Assert.Null(product.StoreProductId);
        Assert.Equal(2, product.Variants.Count);
        Assert.Equal(10, product.Variants.Sum(Variant => Variant.QuantityOnHand));
        Assert.True(await drop.Services.GetRequiredService<IAppearanceResources>().ExistsAsync(owner.EnterpriseId,
            DropAppearanceTargets.Default, "drop_product", product.Id, default));
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        Assert.Equal(1, await db.Set<DropCatalogProduct>().CountAsync());
        var first = await EventAsync(drop, owner);
        var second = await EventAsync(drop, owner);
        await catalog.SendRequiredAsync(new AssociateDropCatalogProductCommand(product.Id, new(first.Id, 10)), default);
        await catalog.SendRequiredAsync(new AssociateDropCatalogProductCommand(product.Id, new(second.Id, 10)), default);
        Publish(first);
        Publish(second);
        await db.SaveChangesAsync();
        var usage = System.Text.Json.JsonSerializer.SerializeToElement(await catalog.SendRequiredAsync(new GetDropCatalogUsageQuery(), default), System.Text.Json.JsonSerializerOptions.Web);
        Assert.Equal(2, usage.GetProperty("publishedPrincipalEvents").GetInt32());
        Assert.Equal(10, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [PostgresFact]
    public async Task Downgrade_blocks_new_entries_keeps_history_and_upgrade_requires_explicit_reactivation()
    {
        var owner = await store.CreateStoreAsync(planId: 3);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var catalog = drop.Services.GetRequiredService<IMediator>();
        var product = await catalog.SendRequiredAsync(new CreateDropCatalogProductCommand(Product("blocked")), default);
        var dropEvent = await EventAsync(drop, owner);
        var participation = await catalog.SendRequiredAsync(new AssociateDropCatalogProductCommand(product.Id, new(dropEvent.Id, 100)), default);
        Publish(dropEvent);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        await db.SaveChangesAsync();
        await store.ChangeContractAsync(owner.EnterpriseId, 2, null);
        await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
        Assert.True(participation.IsContractBlocked);
        Assert.True(product.IsContractBlocked);
        Assert.True(participation.IsActive);
        var failure = await Assert.ThrowsAsync<SecurityFailure>(() => catalog.SendRequiredAsync(new CreateDropCatalogProductCommand(Product("forbidden")), default));
        Assert.Equal("DropContract.ModuleRequired", failure.Code);
        db.ChangeTracker.Clear();
        await store.ChangeContractAsync(owner.EnterpriseId, 3, null);
        var rights = await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
        Assert.True((await db.DropProducts.SingleAsync(Product => Product.Id == participation.Id)).IsContractBlocked);
        await catalog.SendRequiredAsync(new ReactivateDropParticipationsCommand(new(rights.ContractRevision, "selected", [participation.Id])), default);
        Assert.False((await db.DropProducts.SingleAsync(Product => Product.Id == participation.Id)).IsContractBlocked);
        Assert.Equal(10, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        Assert.Contains(await db.Set<DropCommercialActivity>().ToListAsync(), Activity => Activity.Action == "participation_reactivated");
    }

    [PostgresFact]
    public async Task Monotonic_projection_rejects_old_or_conflicting_revision_without_repeating_history()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        var rights = await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
        var count = await db.Set<DropCommercialActivity>().CountAsync();
        await db.ApplyCommercialRightsAsync(rights, default);
        Assert.Equal(count, await db.Set<DropCommercialActivity>().CountAsync());
        await Assert.ThrowsAsync<SecurityFailure>(() => db.ApplyCommercialRightsAsync(rights with { ProductLimit = rights.ProductLimit + 1 }, default));
        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<SecurityFailure>(() => db.ApplyCommercialRightsAsync(rights with { ContractRevision = rights.ContractRevision - 1 }, default));
    }

    [PostgresFact]
    public async Task Creation_total_includes_unpublished_catalog_and_last_slot_is_serialized()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
        using (drop.Services.GetRequiredService<DropCommercialWriteScope>().Begin("catalog"))
        {
            for (var index = 0; index < 99; index++)
            {
                var product = DropCatalogProduct.Create(owner.EnterpriseId, "Draft " + index, "Draft");
                product.Variants.Add(DropCatalogVariant.Create(product, "DRAFT-" + index, "{}", 10, 1));
                db.Add(product);
            }

            await db.SaveChangesAsync();
        }

        async Task<bool> CreateLastAsync(string sku)
        {
            using var scope = drop.Services.CreateScope();

            try
            {
                await scope.ServiceProvider.GetRequiredService<IMediator>().SendRequiredAsync(new CreateDropCatalogProductCommand(Product(sku)), default);

                return true;
            }
            catch (SecurityFailure failure) when (failure.Code == "DropContract.CreationLimit")
            {
                return false;
            }
        }

        var results = await Task.WhenAll(CreateLastAsync("last-a"), CreateLastAsync("last-b"));
        Assert.Single(results, Result => Result);
        Assert.Equal(100, await db.Set<DropCatalogProduct>().CountAsync());
    }

    [PostgresFact]
    public async Task Unpublished_catalog_can_be_reactivated_after_regaining_drop_without_losing_stock()
    {
        var owner = await store.CreateStoreAsync(planId: 5);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var catalog = drop.Services.GetRequiredService<IMediator>();
        var product = await catalog.SendRequiredAsync(new CreateDropCatalogProductCommand(Product("draft")), default);
        await store.ChangeContractAsync(owner.EnterpriseId, 2, null);
        var db = drop.Services.GetRequiredService<DropCommerceDbContext>();
        await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
        Assert.True(product.IsContractBlocked);
        await store.ChangeContractAsync(owner.EnterpriseId, 5, null);
        var rights = await db.RefreshCommercialRightsAsync(owner.EnterpriseId, default);
        await catalog.SendRequiredAsync(new ReactivateDropCatalogProductCommand(product.Id, rights.ContractRevision), default);
        Assert.False((await db.Set<DropCatalogProduct>().SingleAsync()).IsContractBlocked);
        Assert.Equal(10, await db.Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
    }

    [PostgresFact]
    public async Task Rejected_import_restores_source_once_and_tombstone_prevents_a_late_duplicate()
    {
        var owner = await store.CreateStoreAsync(planId: 3, inactiveProducts: 1);
        var productId = await SetStockAsync(owner, 4);
        using var client = store.Client(owner.Token);
        using var response = await client.PostAsJsonAsync($"/api/products/{productId}/drop-stock-transfer", new
        {
            idempotencyKey = "rejected", expectedContractRevision = 2
        });
        response.EnsureSuccessStatusCode();
        var export = (await response.Content.ReadFromJsonAsync<DropStockExport>())!;
        Assert.Equal(0, await SourceStockAsync(owner, productId));
        await store.ChangeContractAsync(owner.EnterpriseId, 2, null);
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var saga = drop.Services.GetRequiredService<IMediator>();
        var rejected = await saga.SendRequiredAsync(new ImportDropStockCommand(export), default);
        Assert.False(rejected.Accepted);
        var options = store.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SharedCommerce.Application.Options.SecurityOptions>>().Value;
        using var service = store.Client();
        service.DefaultRequestHeaders.Add("X-Service-Key", options.InternalServiceKey);
        service.DefaultRequestHeaders.Add("X-Enterprise-Id", owner.EnterpriseId.ToString());
        var returned = new DropStockReturn(export.OperationId, export.OperationId, owner.EnterpriseId, productId, export.Variants);
        using var rollback = await service.PostAsJsonAsync("/api/internal/drop-stock-transfers/return", returned);
        Assert.True(rollback.IsSuccessStatusCode, await rollback.Content.ReadAsStringAsync());
        using var repeated = await service.PostAsJsonAsync("/api/internal/drop-stock-transfers/return", returned);
        repeated.EnsureSuccessStatusCode();
        await store.ChangeContractAsync(owner.EnterpriseId, 3, null);
        Assert.False((await saga.SendRequiredAsync(new ImportDropStockCommand(export), default)).Accepted);
        Assert.Equal(4, await SourceStockAsync(owner, productId));
        Assert.Equal(0, await drop.Services.GetRequiredService<DropCommerceDbContext>().Set<DropCatalogVariant>().CountAsync());
        using var anonymous = store.Client();
        using var denied = await anonymous.GetAsync("/api/internal/drop-sale-payments/pending-receipts");
        Assert.False(denied.IsSuccessStatusCode);
    }

    [PostgresFact]
    public async Task Stock_transfer_and_return_never_offer_the_same_units_in_both_databases()
    {
        var owner = await store.CreateStoreAsync(planId: 3, inactiveProducts: 1);
        var productId = await SetStockAsync(owner, 8);
        using var client = store.Client(owner.Token);
        using var response = await client.PostAsJsonAsync($"/api/products/{productId}/drop-stock-transfer", new
        {
            idempotencyKey = "round-trip", expectedContractRevision = 2
        });
        var error = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, error);
        var export = await response.Content.ReadFromJsonAsync<DropStockExport>();
        Assert.Equal(0, await SourceStockAsync(owner, productId));
        await using var drop = new DropAppearanceTestScope();
        await drop.InitializeAsync(store, owner);
        var saga = drop.Services.GetRequiredService<IMediator>();
        var receipt = await saga.SendRequiredAsync(new ImportDropStockCommand(export!), default);
        Assert.True(receipt.Accepted);
        Assert.Equal(8, await drop.Services.GetRequiredService<DropCommerceDbContext>().Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        await saga.SendRequiredAsync(new ImportDropStockCommand(export!), default);
        Assert.Equal(8, await drop.Services.GetRequiredService<DropCommerceDbContext>().Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        var intent = await saga.SendRequiredAsync(new ReturnDropCatalogStockCommand(receipt.CatalogProductId!.Value), default);
        Assert.Equal(0, await drop.Services.GetRequiredService<DropCommerceDbContext>().Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
        var returned = System.Text.Json.JsonSerializer.Deserialize<DropStockReturn>(intent.SnapshotJson)!;
        var options = store.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SharedCommerce.Application.Options.SecurityOptions>>().Value;
        using var service = store.Client();
        service.DefaultRequestHeaders.Add("X-Service-Key", options.InternalServiceKey);
        service.DefaultRequestHeaders.Add("X-Enterprise-Id", owner.EnterpriseId.ToString());
        using var imported = await service.PostAsJsonAsync("/api/internal/drop-stock-transfers/return", returned);
        Assert.True(imported.IsSuccessStatusCode, await imported.Content.ReadAsStringAsync());
        using var duplicate = await service.PostAsJsonAsync("/api/internal/drop-stock-transfers/return", returned);
        duplicate.EnsureSuccessStatusCode();
        await saga.SendRequiredAsync(new CompleteDropStockReturnCommand(owner.EnterpriseId, returned.OperationId), default);
        Assert.Equal(8, await SourceStockAsync(owner, productId));
        Assert.Equal(0, await drop.Services.GetRequiredService<DropCommerceDbContext>().Set<DropCatalogVariant>().SumAsync(Variant => Variant.QuantityOnHand));
    }

    private static CreateDropCatalogProduct Product(string key)
    {
        return new("Produto " + key, "Descrição", [new(key + "-blue", new() { ["color"] = "blue" }, 10, 5),
            new(key + "-black", new() { ["color"] = "black" }, 12, 5)]);
    }

    private static async Task<DropEvent> EventAsync(DropAppearanceTestScope scope, StoreActor owner)
    {
        var now = DateTime.UtcNow;
        var drop = DropEvent.Create(owner.EnterpriseId, null, "Evento", "event-" + Guid.NewGuid().ToString("N"), "Descrição", "image", "banner",
            1, 10, 0, 0, 10, false, false, now.AddDays(-2), now.AddDays(-1), now.AddHours(-1), now.AddMinutes(-1), now.AddDays(1));
        var db = scope.Services.GetRequiredService<DropCommerceDbContext>();
        db.DropEvents.Add(drop);
        await db.SaveChangesAsync();

        return drop;
    }

    private static void Publish(DropEvent drop)
    {
        drop.Update(drop.EnterpriseId, drop.ProductId, drop.Name, drop.Slug, drop.Description, drop.CoverImageUrl, drop.BannerImageUrl,
            5, drop.TotalUnitsAvailable, 0, 0, drop.Price, false, true, drop.RegistrationStartsAt, drop.RegistrationEndsAt,
            drop.QueueOpensAt, drop.DropStartsAt, drop.DropEndsAt);
    }

    private async Task<long> SetStockAsync(StoreActor owner, int quantity)
    {
        using var scope = store.Services.CreateScope();
        using var identity = scope.ServiceProvider.GetRequiredService<IIdentityOperationScope>().Begin();
        var db = scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>();
        var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.EnterpriseId == owner.EnterpriseId);
        variant.AdjustStock(quantity, false);
        await db.SaveChangesAsync();

        return variant.ProductId;
    }

    private async Task<int> SourceStockAsync(StoreActor owner, long productId)
    {
        using var scope = store.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<StoreCommerceDbContext>().ProductVariants.IgnoreQueryFilters()
            .Where(Variant => Variant.EnterpriseId == owner.EnterpriseId && Variant.ProductId == productId).SumAsync(Variant => Variant.StockQuantity);
    }
}
