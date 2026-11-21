using System.Text.Json;
using DropCommerce.Application.Contracts;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Contracts;

public sealed record CreateDropCatalogVariant(string SKU, Dictionary<string, string> Options, decimal Price, int Quantity);
public sealed record CreateDropCatalogProduct(string Name, string Description, IReadOnlyList<CreateDropCatalogVariant> Variants);
public sealed record AssociateDropCatalogProduct(long DropEventId, int MaxPerCustomer);
public sealed record ReactivateDropParticipations(long ExpectedContractRevision, string Mode, IReadOnlyList<long>? ParticipationIds);

public sealed class DropCatalogService(DropCommerceDbContext db, DropCommercialWriteScope writes,
    ITenantProvider tenants, IHttpContextAccessor http)
{
    private long Tenant => tenants.GetEnterpriseId();

    private void Require(string permission)
    {
        var principal = http.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true || principal.FindFirst(SecurityNames.Actor)?.Value != "user" ||
            principal.FindFirst(SecurityNames.Membership) is null || !SecurityNames.HasPermission(principal, permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }

    public async Task<DropCatalogProduct> CreateAsync(CreateDropCatalogProduct request, CancellationToken ct)
    {
        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await db.RefreshCommercialRightsAsync(tenant, ct);

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 255 || request.Description is null || request.Description.Length > 10000 ||
            request.Variants is null || request.Variants.Count is < 1 or > 200 ||
            request.Variants.Any(Variant => Variant is null || string.IsNullOrWhiteSpace(Variant.SKU) || Variant.SKU.Length > 100 ||
                Variant.Price <= 0 || Variant.Price > 9999999999.99m || decimal.Round(Variant.Price, 2) != Variant.Price || Variant.Quantity is < 0 or > 1000000) ||
            request.Variants.Select(Variant => Variant.SKU.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != request.Variants.Count)
            throw new SecurityFailure(400, "DropCatalog.InvalidVariants", "Informe de uma a duzentas variações com SKUs distintos.");

        using var operation = writes.Begin("catalog");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var product = DropCatalogProduct.Create(tenant, request.Name, request.Description);

        foreach (var variant in request.Variants)
        {
            if (variant.Options is null || variant.Options.Count > 20 || variant.Options.Any(Option =>
                string.IsNullOrWhiteSpace(Option.Key) || Option.Key.Length > 80 || string.IsNullOrWhiteSpace(Option.Value) || Option.Value.Length > 150))
                throw new SecurityFailure(400, "DropCatalog.InvalidOptions", "Opções de variação inválidas.");

            product.Variants.Add(DropCatalogVariant.Create(product, variant.SKU,
                JsonSerializer.Serialize(variant.Options), variant.Price, variant.Quantity));
        }

        db.Add(product);
        await db.SaveChangesAsync(ct);
        db.RecordCommercialActivity(rights, "catalog_created", product.Id, null);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return product;
    }

    public async Task<IReadOnlyList<DropCatalogProduct>> GetProductsAsync(long? afterId, CancellationToken ct)
    {
        Require("drop.events.view");
        await db.RefreshCommercialRightsAsync(Tenant, ct);

        return await db.Set<DropCatalogProduct>().Include(Product => Product.Variants).AsNoTracking()
            .Where(Product => !afterId.HasValue || Product.Id > afterId.Value)
            .OrderBy(Product => Product.Id).Take(50).ToListAsync(ct);
    }

    public async Task<DropProduct> AssociateAsync(long productId, AssociateDropCatalogProduct request, CancellationToken ct)
    {
        Require("drop.events.manage");
        var tenant = Tenant;
        await db.RefreshCommercialRightsAsync(tenant, ct);
        using var operation = writes.Begin("catalog");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var product = await db.Set<DropCatalogProduct>().Include(Product => Product.Variants)
            .SingleOrDefaultAsync(Product => Product.Id == productId, ct)
            ?? throw new SecurityFailure(404, "DropCatalog.NotFound", "Produto DROP não encontrado.");

        if (product.IsContractBlocked || product.IsReturning || product.IsReturned || product.StoreProductId.HasValue && !product.SourceExportId.HasValue)
            throw new SecurityFailure(409, "DropCatalog.Unavailable", "O produto está bloqueado ou aguarda a transferência coordenada de estoque.");

        var drop = await db.DropEvents.SingleOrDefaultAsync(Event => Event.Id == request.DropEventId, ct)
            ?? throw new SecurityFailure(404, "Drop.NotFound", "Evento não encontrado.");
        var existing = await db.DropProducts.Where(Product => Product.DropEventId == drop.Id && Product.CatalogProductId == product.Id)
            .OrderBy(Product => Product.Id).FirstOrDefaultAsync(ct);

        if (existing is not null)
        {
            if (existing.MaxPerCustomer != request.MaxPerCustomer)
                throw new SecurityFailure(409, "DropCatalog.AssociationExists", "A participação já existe com outro limite de compra.");

            return existing;
        }

        var available = product.Variants.Sum(Variant => (long)Variant.QuantityOnHand - Variant.QuantityReserved);

        if (available <= 0 || available > int.MaxValue || request.MaxPerCustomer < 1)
            throw new SecurityFailure(400, "DropCatalog.InvalidAllocation", "Informe estoque positivo e limite de compra válido.");

        var participation = DropProduct.Create(drop.Id, null, product.Variants.First().SKU, (int)available,
            0, request.MaxPerCustomer, product.Variants.Min(Variant => Variant.Price), true);
        participation.BindCatalog(product);
        db.Add(participation);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return participation;
    }

    public async Task<object> GetUsageAsync(CancellationToken ct)
    {
        Require("drop.events.view");
        var rights = await db.RefreshCommercialRightsAsync(Tenant, ct);
        var catalog = await db.Set<DropCatalogProduct>().AsNoTracking().ToListAsync(ct);
        var products = await db.DropProducts.Include(Product => Product.DropEvent).AsNoTracking().ToListAsync(ct);
        var published = products.Where(Product => DropCommerceDbContext.IsPublished(Product.DropEvent) && Product.IsActive).ToList();

        return new
        {
            rights.ContractRevision, rights.AllowsDrop, rights.ProductLimit,
            catalogTotal = catalog.Count,
            canCreate = rights.AllowsDrop && catalog.Count < rights.ProductLimit,
            publishedPrincipalEvents = published.Where(Product => !Product.IsContractBlocked)
                .Select(Product => (Product.DropEventId, Product.CatalogProductId)).Distinct().Count(),
            blockedParticipations = products.Count(Product => Product.IsContractBlocked),
            awaitingStockTransfer = catalog.Count(Product => Product.StoreProductId.HasValue && !Product.SourceExportId.HasValue),
            transferringStock = catalog.Count(Product => Product.IsReturning)
        };
    }

    public async Task<object> ReactivationAsync(ReactivateDropParticipations request, bool confirm, CancellationToken ct)
    {
        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await db.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop || request.ExpectedContractRevision != rights.ContractRevision || request.Mode is not ("oldest" or "selected"))
            throw new SecurityFailure(409, "DropContract.RevisionChanged", "Atualize os direitos e o modo de reativação.");

        using var operation = writes.Begin("reactivation");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var products = await db.DropProducts.Include(Product => Product.DropEvent).Include(Product => Product.CatalogProduct)
            .Where(Product => Product.IsActive).OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToListAsync(ct);
        var used = products.Where(Product => !Product.IsContractBlocked && DropCommerceDbContext.IsPublished(Product.DropEvent))
            .Select(Product => (Product.DropEventId, Product.CatalogProductId)).Distinct().Count();
        var available = Math.Max(0, rights.ProductLimit - used);
        var eligible = products.Where(Product => Product.IsContractBlocked && DropCommerceDbContext.IsPublished(Product.DropEvent) &&
            Product.CatalogProduct is { IsDeleted: false, IsReturning: false, IsReturned: false } &&
            (!Product.CatalogProduct.StoreProductId.HasValue || Product.CatalogProduct.SourceExportId.HasValue)).ToList();
        var selected = request.Mode == "oldest" ? eligible.Take(available).ToList() :
            eligible.Where(Product => request.ParticipationIds?.Contains(Product.Id) == true).ToList();

        if (request.Mode == "selected" && (request.ParticipationIds is null || request.ParticipationIds.Distinct().Count() != request.ParticipationIds.Count ||
            selected.Count != request.ParticipationIds.Count) || selected.Count > available)
            throw new SecurityFailure(409, "DropContract.InvalidSelection", "Selecione participações elegíveis dentro das vagas disponíveis.");

        if (confirm)
        {
            foreach (var product in selected)
            {
                product.CatalogProduct!.Reactivate();
                product.ReactivateForContract();
                db.RecordCommercialActivity(rights, "participation_reactivated", product.CatalogProductId, product.Id);
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        return new { rights.ContractRevision, available, participationIds = selected.Select(Product => Product.Id).ToList(), applied = confirm };
    }

    public async Task ReactivateCatalogAsync(long productId, long expectedRevision, CancellationToken ct)
    {
        Require("drop.events.manage");
        var rights = await db.RefreshCommercialRightsAsync(Tenant, ct);

        if (!rights.AllowsDrop || expectedRevision != rights.ContractRevision)
            throw new SecurityFailure(409, "DropContract.ContractChanged", "Atualize a revisão e os direitos antes de reativar.");

        using var operation = writes.Begin("reactivation");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(Tenant, ct);
        var product = await db.Set<DropCatalogProduct>().SingleOrDefaultAsync(Product => Product.Id == productId, ct)
            ?? throw new SecurityFailure(404, "DropCatalog.NotFound", "Produto não encontrado.");

        if (!product.IsContractBlocked)
            return;

        if (await db.Set<DropCatalogProduct>().CountAsync(Product => !Product.IsContractBlocked, ct) >= rights.ProductLimit)
            throw new SecurityFailure(409, "DropContract.CreationLimit", "Não há vaga para reativar este cadastro.");

        product.Reactivate();
        db.RecordCommercialActivity(rights, "catalog_reactivated", product.Id, null);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task DeleteAsync(long productId, CancellationToken ct)
    {
        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await db.RefreshCommercialRightsAsync(tenant, ct);
        using var operation = writes.Begin("catalog");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.LockCommercialTenantAsync(tenant, ct);
        var product = await db.Set<DropCatalogProduct>().Include(Product => Product.Variants).SingleOrDefaultAsync(Product => Product.Id == productId, ct)
            ?? throw new SecurityFailure(404, "DropCatalog.NotFound", "Produto não encontrado.");
        var participations = await db.DropProducts.Where(Product => Product.CatalogProductId == productId).ToListAsync(ct);
        var ids = participations.Select(Product => Product.Id).ToList();

        if (product.StoreProductId.HasValue && !product.IsReturned || product.Variants.Any(Variant => Variant.QuantityReserved != 0 ||
            !product.IsContractBlocked && Variant.QuantityOnHand != 0) ||
            await db.DropReservations.IgnoreQueryFilters().AnyAsync(Reservation => ids.Contains(Reservation.DropProductId) && Reservation.DropReservationStatusId == 1, ct) ||
            await db.DropOrderItems.IgnoreQueryFilters().AnyAsync(Item => ids.Contains(Item.DropProductId) &&
                (Item.DropOrder.DropOrderStatusId >= 1 && Item.DropOrder.DropOrderStatusId <= 4 ||
                    Item.DropOrder.PaymentOperationId != null && (Item.DropOrder.FinancialReviewCode != null ||
                        Item.DropOrder.DropOrderPaymentStatusId == 1 && Item.DropOrder.PaymentResolution != "not_submitted")), ct))
            throw new SecurityFailure(409, "DropCatalog.StockInUse", "Resolva as reservas e o controle de estoque antes de excluir.");

        product.Delete(DateTime.UtcNow);

        foreach (var participation in participations)
            participation.SoftDelete();

        db.RecordCommercialActivity(rights, "catalog_deleted", product.Id, null);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<object> GetActivityAsync(long? beforeId, CancellationToken ct)
    {
        Require("store.history.view");

        return await db.Set<DropCommercialActivity>().AsNoTracking()
            .Where(Activity => !beforeId.HasValue || Activity.Id < beforeId.Value)
            .OrderByDescending(Activity => Activity.Id).Take(100).ToListAsync(ct);
    }
}
