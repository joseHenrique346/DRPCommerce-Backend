using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private bool GuardInventoryWrites(List<EntityEntry> changes)
    {
        if (_inventoryWrites.EnterpriseId is not long tenant)
        {
            foreach (var entry in changes)
            {
                if (entry.Entity is StockMovement or StockReservation or StockReservationLine)
                    throw new SecurityFailure(403, "Inventory.UseWorkflow", "Use os fluxos específicos de estoque.");

                if (entry.Entity is ProductVariant variant &&
                    ((entry.State == EntityState.Added && (variant.StockQuantity != 0 || variant.ReservedQuantity != 0)) ||
                     (entry.State == EntityState.Modified &&
                      (entry.Property(nameof(ProductVariant.StockQuantity)).IsModified || entry.Property(nameof(ProductVariant.ReservedQuantity)).IsModified))))
                    throw new SecurityFailure(403, "Inventory.UseWorkflow", "O saldo do SKU só pode ser alterado pelos fluxos de estoque.");
            }

            return false;
        }

        foreach (var entry in changes)
        {
            if (entry.Entity is not ITenantEntity scoped || scoped.EnterpriseId != tenant ||
                entry.Entity is not (ProductVariant or StockReservation or StockReservationLine or StockMovement))
                throw new SecurityFailure(403, "Inventory.InvalidScope", "Gravação fora do escopo de estoque autorizado.");

            if (entry.State == EntityState.Deleted ||
                (entry.Entity is StockMovement or StockReservationLine && entry.State != EntityState.Added))
                throw new SecurityFailure(403, "Inventory.ImmutableHistory", "O histórico e as linhas de reserva são imutáveis.");

            if (entry.Entity is ProductVariant &&
                (entry.State != EntityState.Modified || entry.Properties.Any(Property => Property.IsModified &&
                    Property.Metadata.Name is not (nameof(ProductVariant.StockQuantity) or nameof(ProductVariant.ReservedQuantity) or
                        nameof(ProductVariant.Version) or nameof(ProductVariant.UpdatedAt)))))
                throw new SecurityFailure(403, "Inventory.InvalidScope", "O fluxo de estoque não altera os dados do catálogo.");
        }

        return true;
    }
}
