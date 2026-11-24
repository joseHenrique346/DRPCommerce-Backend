using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private async Task GuardDropStockAsync(CancellationToken ct)
    {
        foreach (var entry in ChangeTracker.Entries<DropStockTransfer>().Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (_dropStockTransfers.EnterpriseId != entry.Entity.EnterpriseId || _dropStockTransfers.ProductId != entry.Entity.ProductId ||
                entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Properties.Any(Property => Property.IsModified &&
                    Property.Metadata.Name is not ("DropCatalogProductId" or "ImportedAt" or "ReturnOperationId" or "ReturnedAt" or "UpdatedAt")))
                throw new SecurityFailure(403, "DropStock.UseWorkflow", "Use a transferência coordenada de estoque.");
        }

        foreach (var entry in ChangeTracker.Entries<Product>().Where(Entry => Entry.State == EntityState.Added || Entry.State == EntityState.Modified))
        {
            if (entry.Property(Product => Product.IsTransferredToDrop).IsModified || entry.State == EntityState.Added && entry.Entity.IsTransferredToDrop)
            {
                if (_dropStockTransfers.EnterpriseId != entry.Entity.EnterpriseId || _dropStockTransfers.ProductId != entry.Entity.Id ||
                    entry.State != EntityState.Modified || !OnlyModifiedFields(entry, nameof(Product.IsTransferredToDrop), nameof(Product.UpdatedAt)))
                    throw new SecurityFailure(403, "DropStock.UseWorkflow", "Use a transferência coordenada de estoque.");
            }

            if (entry.Entity.IsTransferredToDrop && (entry.Entity.IsActive || entry.Entity.IsDeleted))
                throw new SecurityFailure(409, "DropStock.OwnedByDrop", "Retorne o estoque do DROP antes de publicar na Vitrine.");
        }

        var changing = ChangeTracker.Entries<ProductVariant>()
            .Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        foreach (var entry in changing)
        {
            var variant = entry.Entity;
            var controlledByDrop = await Products.IgnoreQueryFilters().AnyAsync(Product => Product.Id == variant.ProductId && Product.IsTransferredToDrop, ct);

            if (controlledByDrop && (_dropStockTransfers.EnterpriseId != variant.EnterpriseId || _dropStockTransfers.ProductId != variant.ProductId))
                throw new SecurityFailure(409, "DropStock.OwnedByDrop", "O saldo deste SKU é controlado pelo DROP.");
        }
    }
}
