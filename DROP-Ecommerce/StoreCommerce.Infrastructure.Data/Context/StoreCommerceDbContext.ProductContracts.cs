using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private readonly HashSet<Product> _productContractSystemChanges = [];

    private List<long> ProductContractTenants()
    {
        return ChangeTracker.Entries()
            .Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(Entry => Entry.Entity is Product or ProductVariant or CartItem or OrderItem or StockReservationLine or StockMovement)
            .Select(Entry => Entry.Entity is ITenantEntity scoped ? scoped.EnterpriseId : OrderItemTenant((OrderItem)Entry.Entity))
            .Concat(ChangeTracker.Entries<Enterprise>()
                .Where(Entry => Entry.State == EntityState.Modified && ContractFieldsChanged(Entry))
                .Select(Entry => Entry.Entity.Id))
            .Distinct().OrderBy(Id => Id).ToList();
    }

    private static bool ContractFieldsChanged(EntityEntry<Enterprise> entry)
    {
        return entry.Property(Enterprise => Enterprise.PlanId).IsModified ||
            entry.Property(Enterprise => Enterprise.AddOnId).IsModified ||
            entry.Property(Enterprise => Enterprise.ContractRevision).IsModified ||
            entry.Property(Enterprise => Enterprise.CatalogVersion).IsModified;
    }

    private long OrderItemTenant(OrderItem item)
    {
        return item.Order?.EnterpriseId ?? _orderWorkflow.EnterpriseId ?? _tenantProvider.GetEnterpriseId();
    }

    private async Task GuardProductContractsAsync(CancellationToken ct)
    {
        foreach (var tenant in ProductContractTenants())
        {
            await Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
            var stored = await Enterprises.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(Enterprise => Enterprise.Id == tenant, ct)
                ?? throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");
            var grant = ChangeTracker.Entries<Enterprise>()
                .SingleOrDefault(Entry => Entry.Entity.Id == tenant && Entry.State == EntityState.Modified && ContractFieldsChanged(Entry));
            var contract = grant?.Entity ?? stored;
            var rights = EnterpriseEntitlements.Resolve(contract.PlanId, contract.AddOnId);

            if (_productReactivation.EnterpriseId == tenant &&
                _productReactivation.ExpectedContractRevision != contract.ContractRevision)
                throw new SecurityFailure(409, "Contract.RevisionChanged", "Atualize o contrato antes de reativar produtos.");

            if (grant is not null)
            {
                if (stored.ContractRevision != grant.Property(Enterprise => Enterprise.ContractRevision).OriginalValue)
                    throw new SecurityFailure(409, "Contract.RevisionChanged", "O contrato foi alterado por outra operação.");

                await ReconcileProductContractAsync(tenant, rights, ct);
                AddStoreActivity(tenant, "contract_changed", null, null, "contract_grant", new
                {
                    previousPlanId = stored.PlanId,
                    previousAddOnId = stored.AddOnId,
                    planId = contract.PlanId,
                    addOnId = contract.AddOnId,
                    revision = contract.ContractRevision
                });
            }

            var productEntries = ChangeTracker.Entries<Product>()
                .Where(Entry => Entry.Entity.EnterpriseId == tenant &&
                    Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
            await GuardProductStatesAsync(tenant, contract.ContractRevision, rights, productEntries, ct);
            await GuardVariantStatesAsync(tenant, ct);
            await GuardNewProductOperationsAsync(tenant, rights, ct);
        }

        CaptureProductActivities();
        GuardStoreActivityWrites();
    }

    private async Task ReconcileProductContractAsync(long tenant, EnterpriseEntitlements rights, CancellationToken ct)
    {
        var products = await Products.IgnoreQueryFilters()
            .Where(Product => Product.EnterpriseId == tenant && !Product.IsDeleted).ToListAsync(ct);

        foreach (var added in ChangeTracker.Entries<Product>().Where(Entry =>
            Entry.State == EntityState.Added && Entry.Entity.EnterpriseId == tenant).Select(Entry => Entry.Entity))
            products.Add(added);

        if (!rights.AllowsStore)
        {
            foreach (var product in products.Where(Product => !Product.IsDeleted))
            {
                _productContractSystemChanges.Add(product);
                product.BlockForContract(ProductContractPolicy.StoreModule, DateTime.UtcNow);
            }

            return;
        }

        foreach (var product in products.Where(Product => Product.IsContractBlocked && !Product.IsActive &&
            Product.ContractBlockReason == ProductContractPolicy.StoreModule))
        {
            _productContractSystemChanges.Add(product);
            product.UnblockForContract();
        }

        foreach (var product in ProductContractPolicy.SelectExcess(products, rights.StoreProductLimit))
        {
            _productContractSystemChanges.Add(product);
            product.BlockForContract(ProductContractPolicy.ProductLimit, DateTime.UtcNow);
        }
    }

    private async Task GuardProductStatesAsync(long tenant, long revision, EnterpriseEntitlements rights,
        List<EntityEntry<Product>> entries, CancellationToken ct)
    {
        var created = 0;
        var totalDelta = 0;
        var publicDelta = 0;
        var existingIds = entries.Where(Entry => Entry.State != EntityState.Added).Select(Entry => Entry.Entity.Id).ToList();
        var savedStates = await Products.IgnoreQueryFilters().AsNoTracking().Where(Product =>
            Product.EnterpriseId == tenant && existingIds.Contains(Product.Id))
            .Select(Product => new { Product.Id, Product.IsDeleted, Product.IsActive, Product.IsContractBlocked })
            .ToDictionaryAsync(Product => Product.Id, ct);

        foreach (var entry in entries)
        {
            var product = entry.Entity;

            if (entry.State == EntityState.Deleted)
                throw new SecurityFailure(403, "ProductContract.SoftDeleteRequired", "Use exclusão lógica para preservar o histórico.");

            var added = entry.State == EntityState.Added;
            var wasDeleted = !added && entry.Property(Product => Product.IsDeleted).OriginalValue;
            var wasBlocked = !added && entry.Property(Product => Product.IsContractBlocked).OriginalValue;
            var wasActive = !added && entry.Property(Product => Product.IsActive).OriginalValue;

            if (!added && (!savedStates.TryGetValue(product.Id, out var saved) ||
                saved.IsDeleted != wasDeleted || saved.IsActive != wasActive || saved.IsContractBlocked != wasBlocked))
                throw new SecurityFailure(409, "ProductContract.CatalogChanged", "O produto mudou. Atualize a consulta.");

            var isDeleting = !added && !wasDeleted && product.IsDeleted;
            var isRestoring = !added && wasDeleted && !product.IsDeleted;
            var blockChanged = !added && (entry.Property(Product => Product.IsContractBlocked).IsModified ||
                entry.Property(Product => Product.ContractBlockReason).IsModified ||
                entry.Property(Product => Product.ContractBlockedAt).IsModified);
            var policyChange = _productContractSystemChanges.Contains(product);
            var reactivating = _productReactivation.EnterpriseId == tenant &&
                _productReactivation.ExpectedContractRevision == revision && wasBlocked && !product.IsContractBlocked;

            if (!added && entry.Property(Product => Product.CreatedAt).IsModified)
                throw new SecurityFailure(403, "ProductContract.ImmutableCreationDate", "A antiguidade do produto é controlada pelo servidor.");

            if (added && (product.IsContractBlocked || product.IsDeleted) || blockChanged && !policyChange && !reactivating)
                throw new SecurityFailure(403, "ProductContract.UseReactivation", "Use o fluxo próprio de bloqueio e reativação.");

            if ((wasBlocked || product.IsContractBlocked) && !isDeleting &&
                (!OnlyProductBlockFields(entry) || !policyChange && !reactivating) &&
                !(_dropStockTransfers.EnterpriseId == tenant && _dropStockTransfers.ProductId == product.Id &&
                    OnlyModifiedFields(entry, nameof(Product.IsTransferredToDrop), nameof(Product.UpdatedAt))))
                throw BlockedProduct();

            if (isDeleting)
            {
                if (!OnlyModifiedFields(entry, nameof(Product.IsDeleted), nameof(Product.DeletedAt), nameof(Product.UpdatedAt)))
                    throw BlockedProduct();

                var balances = await ProductVariants.IgnoreQueryFilters().Where(Variant =>
                    Variant.EnterpriseId == tenant && Variant.ProductId == product.Id && !Variant.IsDeleted)
                    .Select(Variant => new { Variant.StockQuantity, Variant.ReservedQuantity }).ToListAsync(ct);

                if (balances.Any(Balance => Balance.ReservedQuantity != 0 ||
                    !wasBlocked && Balance.StockQuantity != 0))
                    throw new SecurityFailure(409, "Inventory.StockInUse", "Há estoque ou reservas incompatíveis com a exclusão.");

                if (await StockReservationLines.IgnoreQueryFilters().AnyAsync(Line =>
                    Line.EnterpriseId == tenant && Line.ProductVariant.ProductId == product.Id &&
                    Line.StockReservation.Status == StockReservationStatus.Reserved, ct))
                    throw new SecurityFailure(409, "Inventory.StockInUse", "Há reservas ativas para esse produto.");
            }

            if (added && !product.IsDeleted || isRestoring)
                created++;

            totalDelta += (product.IsDeleted ? 0 : 1) - (added || wasDeleted ? 0 : 1);
            publicDelta += (product.IsActive && !product.IsDeleted && !product.IsContractBlocked ? 1 : 0) -
                (!added && wasActive && !wasDeleted && !wasBlocked ? 1 : 0);
        }

        if (created > 0)
        {
            var currentCount = await Products.IgnoreQueryFilters()
                .LongCountAsync(Product => Product.EnterpriseId == tenant && !Product.IsDeleted, ct);

            if (!rights.AllowsStore || currentCount + totalDelta > rights.StoreProductLimit)
                throw new SecurityFailure(409, "ProductContract.CreationLimit",
                    "O total de produtos cadastrados, incluindo arquivados, excede o limite contratado.");
        }

        if (publicDelta > 0)
        {
            var publicCount = await Products.IgnoreQueryFilters().LongCountAsync(Product =>
                Product.EnterpriseId == tenant && Product.IsActive && !Product.IsDeleted && !Product.IsContractBlocked, ct);

            if (!rights.AllowsStore || publicCount + publicDelta > rights.StoreProductLimit)
                throw new SecurityFailure(409, "ProductContract.PublicationLimit", "Não há capacidade para publicar esses produtos.");
        }
    }

    private static bool OnlyProductBlockFields(EntityEntry entry)
    {
        return OnlyModifiedFields(entry, nameof(Product.IsContractBlocked), nameof(Product.ContractBlockReason),
            nameof(Product.ContractBlockedAt), nameof(Product.UpdatedAt));
    }

    private static bool OnlyModifiedFields(EntityEntry entry, params string[] allowed)
    {
        return entry.Properties.All(Property => !Property.IsModified || allowed.Contains(Property.Metadata.Name));
    }

    private static SecurityFailure BlockedProduct()
    {
        return new SecurityFailure(409, "ProductContract.Blocked",
            "Produto bloqueado pelo contrato. São permitidas consulta, exclusão lógica e reativação autorizada.");
    }

    private async Task<Product?> ReadProductStateAsync(long tenant, long productId, CancellationToken ct)
    {
        var local = ChangeTracker.Entries<Product>().FirstOrDefault(Entry =>
            Entry.Entity.EnterpriseId == tenant && Entry.Entity.Id == productId && Entry.State != EntityState.Unchanged);

        if (local is not null)
            return local.Entity;

        return await Products.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Product => Product.Id == productId && Product.EnterpriseId == tenant, ct);
    }

    private async Task GuardVariantStatesAsync(long tenant, CancellationToken ct)
    {
        var variants = ChangeTracker.Entries<ProductVariant>().Where(Entry =>
            Entry.Entity.EnterpriseId == tenant &&
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        foreach (var entry in variants)
        {
            var variant = entry.Entity;
            var product = variant.ProductId > 0
                ? await ReadProductStateAsync(tenant, variant.ProductId, ct)
                : variant.Product;

            if (product is null)
                throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado.");

            if (variant.Product is null && variant.ProductId > 0)
                await entry.Reference(Variant => Variant.Product).Query().IgnoreQueryFilters()
                    .Where(Product => Product.EnterpriseId == tenant).LoadAsync(ct);

            if (entry.State == EntityState.Deleted)
                throw new SecurityFailure(403, "ProductContract.SoftDeleteRequired", "Use exclusão lógica para preservar os SKUs.");

            if (product.IsDeleted && variant.IsDeleted && OnlyModifiedFields(entry,
                nameof(ProductVariant.IsDeleted), nameof(ProductVariant.DeletedAt), nameof(ProductVariant.Version),
                nameof(ProductVariant.UpdatedAt), nameof(ProductVariant.IsActive)))
                continue;

            if (!product.IsContractBlocked)
                continue;

            var technicalStockChange = IsTechnicalStockChange(entry) &&
                (_orderWorkflow.EnterpriseId == tenant || _inventoryWrites.EnterpriseId == tenant) &&
                !ChangeTracker.Entries<StockMovement>().Any(Movement => Movement.State == EntityState.Added &&
                    Movement.Entity.ProductVariantId == variant.Id && Movement.Entity.Type == StockMovementType.Adjustment);

            if (!technicalStockChange)
                throw BlockedProduct();
        }
    }

    private static bool IsTechnicalStockChange(EntityEntry<ProductVariant> entry)
    {
        return entry.State == EntityState.Modified && OnlyModifiedFields(entry,
            nameof(ProductVariant.StockQuantity), nameof(ProductVariant.ReservedQuantity),
            nameof(ProductVariant.Version), nameof(ProductVariant.UpdatedAt));
    }

    private async Task GuardNewProductOperationsAsync(long tenant, EnterpriseEntitlements rights, CancellationToken ct)
    {
        var productIds = ChangeTracker.Entries<CartItem>().Where(Entry =>
                Entry.Entity.EnterpriseId == tenant && (Entry.State == EntityState.Added ||
                Entry.State == EntityState.Modified && (Entry.Property(Item => Item.Quantity).IsModified ||
                    Entry.Property(Item => Item.IsSelected).IsModified && Entry.Entity.IsSelected ||
                    Entry.Property(Item => Item.IsAwaitingStock).IsModified && Entry.Entity.IsAwaitingStock)))
            .Select(Entry => Entry.Entity.ProductId)
            .Concat(ChangeTracker.Entries<OrderItem>().Where(Entry =>
                OrderItemTenant(Entry.Entity) == tenant && Entry.Entity.ProductId.HasValue &&
                (Entry.State == EntityState.Added || Entry.State == EntityState.Modified &&
                    (Entry.Property(Item => Item.Quantity).IsModified || Entry.Property(Item => Item.ProductId).IsModified ||
                     Entry.Property(Item => Item.ProductVariantId).IsModified)))
                .Select(Entry => Entry.Entity.ProductId!.Value)).ToHashSet();

        var completingExistingOrder = _orderWorkflow.EnterpriseId == tenant &&
            ChangeTracker.Entries<Order>().Any(Entry =>
                Entry.Entity.EnterpriseId == tenant && Entry.Entity.Id > 0 && Entry.State == EntityState.Modified);

        if (!completingExistingOrder)
        {
            var variantIds = ChangeTracker.Entries<StockReservationLine>().Where(Entry =>
                Entry.Entity.EnterpriseId == tenant && Entry.State == EntityState.Added)
                .Select(Entry => Entry.Entity.ProductVariantId).ToList();
            var reservedProducts = await ProductVariants.IgnoreQueryFilters().Where(Variant =>
                Variant.EnterpriseId == tenant && variantIds.Contains(Variant.Id)).Select(Variant => Variant.ProductId).ToListAsync(ct);
            productIds.UnionWith(reservedProducts);
        }

        foreach (var productId in productIds)
        {
            var product = await ReadProductStateAsync(tenant, productId, ct);

            if (!rights.AllowsStore || product is null || product.IsDeleted || !product.IsActive || product.IsContractBlocked)
                throw new SecurityFailure(409, "ProductContract.Unavailable", "Um produto está indisponível para novas operações.");
        }
    }
}
