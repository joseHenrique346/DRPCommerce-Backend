using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Context;

public partial class DropCommerceDbContext
{
    private readonly HashSet<object> _commercialGenerated = [];

    public Task LockCommercialTenantAsync(long tenant, CancellationToken ct)
    {
        return Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"drop-contract:" + tenant}, 0))", ct);
    }

    public async Task<DropCommercialRights> RefreshCommercialRightsAsync(long tenant, CancellationToken ct)
    {
        var rights = await _commercialReader.RightsAsync(tenant, ct);
        await ApplyCommercialRightsAsync(rights, ct);

        return rights;
    }

    public async Task ApplyCommercialRightsAsync(DropCommercialRights rights, CancellationToken ct)
    {
        if (ChangeTracker.HasChanges())
            throw new InvalidOperationException("Sincronize o contrato antes de alterar o catálogo.");

        using var operation = _commercialWrites.Begin("sync");
        await using var tx = await Database.BeginTransactionAsync(ct);
        await LockCommercialTenantAsync(rights.EnterpriseId, ct);
        await ProjectCommercialRightsAsync(rights, ct);
        await SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task ProjectCommercialRightsAsync(DropCommercialRights rights, CancellationToken ct)
    {
        if (rights.EnterpriseId < 1 || rights.ContractRevision < 1 || rights.CatalogVersion != 1 ||
            rights.ProductLimit < 0 || !rights.AllowsDrop && rights.ProductLimit != 0)
            throw new SecurityFailure(503, "DropContract.InvalidSnapshot", "Contrato comercial inválido.");

        var state = await Set<DropContractState>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(State => State.EnterpriseId == rights.EnterpriseId, ct);

        if (state is not null && state.Revision > rights.ContractRevision)
            throw new SecurityFailure(409, "DropContract.StaleRevision", "A revisão comercial já foi substituída.");

        if (state is not null && state.Revision == rights.ContractRevision)
        {
            if (state.AllowsDrop != rights.AllowsDrop || state.ProductLimit != rights.ProductLimit || state.CatalogVersion != rights.CatalogVersion)
                throw new SecurityFailure(503, "DropContract.ConflictingSnapshot", "A mesma revisão contém direitos divergentes.");

            return;
        }

        state ??= DropContractState.Create(rights.EnterpriseId);

        if (state.Id == 0)
            Add(state);

        _commercialGenerated.Add(state);
        state.Apply(rights.ContractRevision, rights.CatalogVersion, rights.AllowsDrop, rights.ProductLimit, DateTime.UtcNow);
        var catalog = await Set<DropCatalogProduct>().IgnoreQueryFilters()
            .Where(Product => Product.EnterpriseId == rights.EnterpriseId && !Product.IsDeleted)
            .OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToListAsync(ct);

        foreach (var product in catalog.Skip(rights.AllowsDrop ? rights.ProductLimit : 0).Where(Product => !Product.IsContractBlocked))
        {
            product.Block();
            _commercialGenerated.Add(product);
            CommercialActivity(rights, "catalog_blocked", product.Id, null);
        }

        var participations = await DropProducts.IgnoreQueryFilters().Include(Product => Product.DropEvent)
            .Include(Product => Product.CatalogProduct)
            .Where(Product => Product.DropEvent.EnterpriseId == rights.EnterpriseId && !Product.IsDeleted && !Product.DropEvent.IsDeleted)
            .OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToListAsync(ct);
        var groups = participations.Where(Product => IsPublished(Product.DropEvent) && Product.IsActive && !Product.IsContractBlocked)
            .GroupBy(Product => (Product.DropEventId, Principal(Product))).ToList();
        var keep = rights.AllowsDrop ? rights.ProductLimit : 0;

        foreach (var product in participations.Where(Product => !rights.AllowsDrop ||
            Product.CatalogProduct?.IsContractBlocked == true || groups.Skip(keep).Any(Group => Group.Contains(Product))))
        {
            if (product.IsContractBlocked)
                continue;

            product.BlockForContract();
            _commercialGenerated.Add(product);
            CommercialActivity(rights, "participation_blocked", product.CatalogProductId, product.Id);
        }

        CommercialActivity(rights, "contract_applied", null, null);
    }

    private void CommercialActivity(DropCommercialRights rights, string action, long? catalogId, long? participationId)
    {
        var activity = DropCommercialActivity.Record(rights.EnterpriseId, rights.ContractRevision, action, DateTime.UtcNow, catalogId, participationId);
        _commercialGenerated.Add(activity);
        Add(activity);
    }

    public void RecordCommercialActivity(DropCommercialRights rights, string action, long? catalogId, long? participationId)
    {
        if (_commercialWrites.Purpose is not ("catalog" or "reactivation" or "transfer"))
            throw new InvalidOperationException("Registro comercial fora do workflow.");

        CommercialActivity(rights, action, catalogId, participationId);
    }

    private async Task GuardCommercialContractsAsync(CancellationToken ct)
    {
        if (_commercialWrites.Purpose is "sales" or "expiration")
        {
            await GuardPurchaseWorkflowAsync(ct);

            return;
        }

        var changed = ChangeTracker.Entries().Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        foreach (var entry in changed)
        {
            if (entry.Entity is QueueEntry or QueueSession or DropReservation or DropReservationLine or DropOrder or DropOrderItem or DropTransaction)
                throw new SecurityFailure(403, "DropPurchase.UseWorkflow", "Use a fila e as reservas controladas; o CRUD não confirma compras.");

            if (entry.Entity is DropContractState or DropCommercialActivity)
            {
                if (!_commercialGenerated.Contains(entry.Entity) || entry.State == EntityState.Deleted)
                    throw new SecurityFailure(403, "DropContract.UseWorkflow", "Use o fluxo comercial do DROP.");
            }

            if (entry.Entity is DropCatalogVariant && _commercialWrites.Purpose is not ("catalog" or "transfer") ||
                entry.Entity is DropCatalogVariant && entry.State != EntityState.Added && _commercialWrites.Purpose != "transfer")
                throw new SecurityFailure(403, "DropStock.UseWorkflow", "Use o fluxo de estoque do DROP.");
        }

        if (_commercialWrites.Purpose == "transfer" && changed.Any(Entry => Entry.Entity is not
            (DropCatalogProduct or DropCatalogVariant or DropStockImportReceipt or DropStockReturnIntent or DropCommercialActivity)))
            throw new SecurityFailure(403, "DropStock.InvalidScope", "A transferência não altera outros agregados.");

        foreach (var entry in changed.Where(Entry => Entry.Entity is DropStockImportReceipt or DropStockReturnIntent))
        {
            if (_commercialWrites.Purpose != "transfer" || entry.State == EntityState.Deleted ||
                entry.Entity is DropStockImportReceipt && entry.State != EntityState.Added ||
                entry.Entity is DropStockReturnIntent && entry.State == EntityState.Modified && entry.Properties.Any(Property =>
                    Property.IsModified && Property.Metadata.Name is not ("CompletedAt" or "UpdatedAt")))
                throw new SecurityFailure(403, "DropStock.ImmutableIntent", "Use as intenções de transferência imutáveis.");
        }

        if (_commercialWrites.Purpose == "sync")
        {
            if (changed.Any(Entry => !_commercialGenerated.Contains(Entry.Entity)))
                throw new SecurityFailure(403, "DropContract.InvalidSynchronization", "A sincronização não pode alterar outros dados.");

            return;
        }

        var relevant = changed.Where(Entry => Entry.Entity is DropEvent or DropProduct or DropCatalogProduct or DropCatalogVariant or
            QueueEntry or QueueSession or DropRegistration or DropReservation or DropOrder or DropOrderItem or DropTransaction).ToList();

        if (relevant.Count == 0)
            return;

        var tenant = _commercialWrites.EnterpriseId ?? _tenantProvider.GetEnterpriseId();
        await LockCommercialTenantAsync(tenant, ct);
        var rights = await _commercialReader.RightsAsync(tenant, ct);
        await ProjectCommercialRightsAsync(rights, ct);

        foreach (var entry in relevant)
        {
            var deleting = entry.State == EntityState.Deleted || entry.Entity is DropCatalogProduct { IsDeleted: true } ||
                entry.Entity is DropProduct { IsDeleted: true } || entry.Entity is DropEvent { IsDeleted: true };

            if (!rights.AllowsDrop && !deleting && _commercialWrites.Purpose != "transfer" && (entry.Entity is DropEvent or DropProduct or DropCatalogProduct or DropCatalogVariant ||
                entry.State == EntityState.Added))
                throw new SecurityFailure(403, "DropContract.ModuleRequired", "O contrato atual não permite novas operações DROP.");

            if (entry.Entity is DropCatalogProduct catalog)
            {
                if (catalog.EnterpriseId != tenant || _commercialWrites.Purpose is not ("catalog" or "reactivation" or "transfer"))
                    throw new SecurityFailure(403, "DropCatalog.UseWorkflow", "Use o catálogo da própria loja.");

                if (entry.State == EntityState.Added && _commercialWrites.Purpose != "transfer" &&
                    (catalog.StoreProductId.HasValue || catalog.SourceExportId.HasValue || catalog.IsReturning || catalog.IsReturned || catalog.IsContractBlocked) ||
                    entry.State == EntityState.Modified && _commercialWrites.Purpose != "transfer" && entry.Properties.Any(Property => Property.IsModified &&
                        Property.Metadata.Name is "EnterpriseId" or "StoreProductId" or "SourceExportId" or "SourceVariantIdsJson" or "CreatedAt" or "IsReturning" or "IsReturned"))
                    throw new SecurityFailure(403, "DropCatalog.ImmutableSource", "A origem e a antiguidade só mudam na transferência coordenada.");

                if (entry.State != EntityState.Added && !deleting && _commercialWrites.Purpose is not ("reactivation" or "transfer") &&
                    (catalog.IsContractBlocked || (bool)entry.Property("IsContractBlocked").OriginalValue!))
                    throw new SecurityFailure(409, "DropContract.Blocked", "O cadastro está bloqueado pelo contrato.");

                if (entry.Property("IsContractBlocked").IsModified && !_commercialGenerated.Contains(catalog) && _commercialWrites.Purpose != "reactivation")
                    throw new SecurityFailure(403, "DropContract.UseReactivation", "Use a reativação explícita.");
            }

            if (entry.Entity is DropProduct product)
            {
                if (product.CatalogProduct is not null && product.ProductId != product.CatalogProduct.StoreProductId)
                    throw new SecurityFailure(409, "DropCatalog.SourceMismatch", "A participação não pode substituir a origem do principal.");

                if (entry.State == EntityState.Added && (product.CatalogProductId is null || _commercialWrites.Purpose != "catalog"))
                    throw new SecurityFailure(409, "DropCatalog.CatalogRequired", "Cadastre o principal e associe-o ao evento pelo catálogo DROP.");

                if (entry.State != EntityState.Added && !deleting &&
                    (product.IsContractBlocked || (bool)entry.Property("IsContractBlocked").OriginalValue!) &&
                    _commercialWrites.Purpose != "reactivation")
                    throw new SecurityFailure(409, "DropContract.Blocked", "A participação está bloqueada pelo contrato.");

                if (entry.Property("IsContractBlocked").IsModified && !_commercialGenerated.Contains(product) && _commercialWrites.Purpose != "reactivation")
                    throw new SecurityFailure(403, "DropContract.UseReactivation", "Use a reativação explícita de participações.");

                if (entry.State == EntityState.Modified && (entry.Property("UnitsSold").IsModified || entry.Property("CatalogProductId").IsModified || entry.Property("ProductId").IsModified))
                    throw new SecurityFailure(403, "DropStock.UseWorkflow", "O saldo e o principal não podem ser substituídos por edição genérica.");
            }

            if (entry.Entity is DropCatalogVariant variant && variant.CatalogProduct.EnterpriseId != tenant)
                throw new SecurityFailure(403, "DropCatalog.TenantMismatch", "Variação de outra loja.");
        }

        var savedCatalog = await Set<DropCatalogProduct>().IgnoreQueryFilters().AsNoTracking()
            .Where(Product => Product.EnterpriseId == tenant && !Product.IsDeleted).ToListAsync(ct);
        var catalogEntries = relevant.Where(Entry => Entry.Entity is DropCatalogProduct).ToList();
        var creating = catalogEntries.Count(Entry => Entry.State == EntityState.Added);

        if (creating > 0 && savedCatalog.Count + creating > rights.ProductLimit)
            throw new SecurityFailure(409, "DropContract.CreationLimit", "O total de cadastros DROP atingiu o limite contratado.");

        var unblocking = catalogEntries.Count(Entry => Entry.State == EntityState.Modified &&
            (bool)Entry.Property("IsContractBlocked").OriginalValue! && !((DropCatalogProduct)Entry.Entity).IsContractBlocked);

        if (unblocking > 0 && savedCatalog.Count(Product => !Product.IsContractBlocked) + unblocking > rights.ProductLimit)
            throw new SecurityFailure(409, "DropContract.CreationLimit", "Não há vagas para reativar os cadastros selecionados.");

        var events = await DropEvents.IgnoreQueryFilters().Where(Event => Event.EnterpriseId == tenant).ToListAsync(ct);

        foreach (var entry in relevant.Where(Entry => Entry.Entity is DropEvent && Entry.State == EntityState.Added))
            events.Add((DropEvent)entry.Entity);

        var products = await DropProducts.IgnoreQueryFilters().Include(Product => Product.CatalogProduct)
            .Where(Product => Product.DropEvent.EnterpriseId == tenant).ToListAsync(ct);

        foreach (var entry in relevant.Where(Entry => Entry.Entity is DropProduct && Entry.State == EntityState.Added))
            products.Add((DropProduct)entry.Entity);

        var activeEvents = events.Where(IsPublished).Select(Event => Event.Id).ToHashSet();
        var publicProducts = products.Where(Product => !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked &&
            Product.CatalogProduct is { IsDeleted: false, IsContractBlocked: false } && activeEvents.Contains(Product.DropEventId)).ToList();

        if (publicProducts.Select(Product => (Product.DropEventId, Principal(Product))).Distinct().Count() > rights.ProductLimit)
            throw new SecurityFailure(409, "DropContract.PublicationLimit", "As participações publicadas ou agendadas ultrapassam o plano.");

        foreach (var entry in relevant.Where(Entry => Entry.Entity is DropEvent && !((DropEvent)Entry.Entity).IsDeleted))
        {
            var drop = (DropEvent)entry.Entity;

            if (IsPublished(drop) && !publicProducts.Any(Product => Product.DropEventId == drop.Id && !Product.CatalogProduct!.IsReturning &&
                !Product.CatalogProduct.IsReturned && (!Product.CatalogProduct.StoreProductId.HasValue || Product.CatalogProduct.SourceExportId.HasValue)))
                throw new SecurityFailure(409, "DropCatalog.ProductsRequired", "Adicione produtos próprios com estoque antes de publicar o evento.");
        }
    }

    public static bool IsPublished(DropEvent drop)
    {
        return DropEventPublication.IsPublished(drop);
    }

    private static long Principal(DropProduct product)
    {
        return product.CatalogProductId ?? product.ProductId ?? -product.Id;
    }
}
