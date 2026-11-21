using System.Text.Json;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Contracts;

public sealed class DropStockSagaService(DropCommerceDbContext db, DropCommercialWriteScope writes,
    IStoreCommercialReader contracts, ITenantProvider tenants, IHttpContextAccessor http)
{
    public async Task<DropStockImportReceipt> ImportAsync(DropStockExport export, CancellationToken ct)
    {
        if (export.EnterpriseId < 1 || export.ProductId < 1 || export.OperationId == Guid.Empty ||
            string.IsNullOrWhiteSpace(export.Name) || export.Name.Length > 255 || export.Description is null || export.Description.Length > 10000 ||
            export.Variants is null || export.Variants.Count is < 1 or > 200 ||
            export.Variants.Any(Variant => Variant.SourceVariantId < 1 || string.IsNullOrWhiteSpace(Variant.SKU) || Variant.SKU.Length > 100 ||
                Variant.Quantity is < 0 or > 1000000 || Variant.Price <= 0 || Variant.Price > 9999999999.99m) ||
            export.Variants.Select(Variant => Variant.SourceVariantId).Distinct().Count() != export.Variants.Count)
            throw new SecurityFailure(409, "DropStock.InvalidExport", "A transferência de origem é inválida.");

        var rights = await contracts.RightsAsync(export.EnterpriseId, ct);
        await db.ApplyCommercialRightsAsync(rights, ct);
        using var operation = writes.Begin("transfer", export.EnterpriseId);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(export.EnterpriseId, ct);
        var receipt = await db.Set<DropStockImportReceipt>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Receipt => Receipt.OperationId == export.OperationId, ct);

        if (receipt is not null)
        {
            if (receipt.EnterpriseId != export.EnterpriseId)
                throw new SecurityFailure(409, "DropStock.TenantMismatch", "Transferência de outra loja.");

            return receipt;
        }

        var product = await db.Set<DropCatalogProduct>().IgnoreQueryFilters().Include(Product => Product.Variants)
            .SingleOrDefaultAsync(Product => Product.EnterpriseId == export.EnterpriseId && Product.StoreProductId == export.ProductId, ct);
        var total = await db.Set<DropCatalogProduct>().IgnoreQueryFilters().CountAsync(Product =>
            Product.EnterpriseId == export.EnterpriseId && !Product.IsDeleted, ct);
        string? rejection = !rights.AllowsDrop ? "DropContract.ModuleRequired" :
            product is null && total >= rights.ProductLimit ? "DropContract.CreationLimit" :
            product?.IsDeleted == true ? "DropCatalog.Deleted" :
            product is not null && export.Variants.Any(Source => product.Variants.Any(Variant =>
                Variant.SKU == Source.SKU && Variant.SourceVariantId.HasValue && Variant.SourceVariantId != Source.SourceVariantId))
                ? "DropStock.VariantMappingConflict" : null;

        if (rejection is not null)
        {
            receipt = DropStockImportReceipt.Record(export.OperationId, export.EnterpriseId, null, rejection);
            db.Add(receipt);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return receipt;
        }

        if (product is null)
        {
            product = DropCatalogProduct.Create(export.EnterpriseId, export.Name, export.Description);
            db.Add(product);
        }

        product.Import(export.ProductId, export.OperationId, export.Name, export.Description,
            export.Variants.Select(Variant => Variant.SourceVariantId).ToArray(), export.OriginalCreatedAt);

        foreach (var source in export.Variants)
        {
            using var json = JsonDocument.Parse(source.OptionsJson);

            if (json.RootElement.ValueKind != JsonValueKind.Object)
                throw new SecurityFailure(409, "DropStock.InvalidOptions", "As opções do SKU de origem são inválidas.");

            var variant = product.Variants.SingleOrDefault(Variant => Variant.SourceVariantId == source.SourceVariantId) ??
                product.Variants.SingleOrDefault(Variant => Variant.SKU == source.SKU && !Variant.SourceVariantId.HasValue);

            if (variant is null)
            {
                variant = DropCatalogVariant.Create(product, source.SKU, source.OptionsJson, source.Price, 0);
                product.Variants.Add(variant);
            }

            variant.Import(source.SourceVariantId, source.SKU, source.OptionsJson, source.Price, source.Quantity);
        }

        receipt = DropStockImportReceipt.Record(export.OperationId, export.EnterpriseId, product, null);
        db.Add(receipt);
        await db.SaveChangesAsync(ct);
        db.RecordCommercialActivity(rights, "stock_imported", product.Id, null);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return receipt;
    }

    public async Task<DropStockReturnIntent> ReturnAsync(long productId, CancellationToken ct)
    {
        var user = http.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true || !SecurityNames.HasPermission(user, "drop.events.manage"))
            throw new SecurityFailure(403, "DropStock.PermissionRequired", "Permissão de gestão DROP necessária.");

        var tenant = tenants.GetEnterpriseId();
        await db.RefreshCommercialRightsAsync(tenant, ct);
        using var operation = writes.Begin("transfer", tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var existing = await db.Set<DropStockReturnIntent>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Return => Return.EnterpriseId == tenant && Return.CatalogProductId == productId && Return.CompletedAt == null, ct);

        if (existing is not null)
            return existing;

        var product = await db.Set<DropCatalogProduct>().Include(Product => Product.Variants)
            .SingleOrDefaultAsync(Product => Product.Id == productId, ct)
            ?? throw new SecurityFailure(404, "DropStock.NotFound", "Produto não encontrado.");

        var future = await db.DropProducts.IgnoreQueryFilters().AnyAsync(Product => Product.CatalogProductId == product.Id &&
            Product.DropEvent.EnterpriseId == tenant && !Product.IsDeleted && !Product.DropEvent.IsDeleted &&
            Product.DropEvent.DropEndsAt > DateTime.UtcNow && Product.DropEvent.DropEventStatusId != 7 && Product.DropEvent.DropEventStatusId != 8, ct);
        var reservations = await db.DropReservations.IgnoreQueryFilters().AnyAsync(Reservation =>
            Reservation.DropProduct.CatalogProductId == product.Id && Reservation.DropEvent.EnterpriseId == tenant &&
            Reservation.DropReservationStatusId == 1, ct);
        var orders = await db.DropOrderItems.IgnoreQueryFilters().AnyAsync(Item => Item.DropProduct.CatalogProductId == product.Id &&
            Item.DropOrder.DropEvent.EnterpriseId == tenant && (Item.DropOrder.DropOrderStatusId >= 1 && Item.DropOrder.DropOrderStatusId <= 4 ||
                Item.DropOrder.PaymentOperationId != null && (Item.DropOrder.FinancialReviewCode != null ||
                    Item.DropOrder.DropOrderPaymentStatusId == 1 && Item.DropOrder.PaymentResolution != "not_submitted")), ct);

        if (future || reservations || orders || product.Variants.Any(Variant => Variant.QuantityReserved != 0))
            throw new SecurityFailure(409, "DropStock.ActiveOperations", "Encerre os eventos e resolva reservas e pedidos antes do retorno.");

        if (!product.StoreProductId.HasValue || !product.SourceExportId.HasValue)
            throw new SecurityFailure(409, "DropStock.NoSource", "O produto não possui origem Store para retorno.");

        var variantIds = JsonSerializer.Deserialize<long[]>(product.SourceVariantIdsJson)!;
        var variants = product.Variants.Where(Variant => Variant.SourceVariantId.HasValue && variantIds.Contains(Variant.SourceVariantId.Value))
            .Select(Variant => new DropStockVariant(Variant.SourceVariantId!.Value, Variant.SKU, Variant.OptionsJson, Variant.Price, Variant.Withdraw())).ToList();
        product.StartReturn();
        var snapshot = new DropStockReturn(Guid.NewGuid(), product.SourceExportId.Value, tenant, product.StoreProductId.Value, variants);
        var intent = DropStockReturnIntent.Create(snapshot.OperationId, tenant, product.Id, JsonSerializer.Serialize(snapshot));
        db.Add(intent);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return intent;
    }

    public async Task CompleteReturnAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        using var operation = writes.Begin("transfer", tenant);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var intent = await db.Set<DropStockReturnIntent>().IgnoreQueryFilters().SingleAsync(Return => Return.EnterpriseId == tenant && Return.OperationId == operationId, ct);
        var product = await db.Set<DropCatalogProduct>().IgnoreQueryFilters().SingleAsync(Product => Product.Id == intent.CatalogProductId && Product.EnterpriseId == tenant, ct);
        intent.Complete(DateTime.UtcNow);
        product.CompleteReturn();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
