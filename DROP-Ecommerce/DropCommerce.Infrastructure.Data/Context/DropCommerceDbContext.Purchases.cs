using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Context;

public partial class DropCommerceDbContext
{
    private async Task GuardPurchaseWorkflowAsync(CancellationToken ct)
    {
        var tenant = _commercialWrites.EnterpriseId ?? throw new InvalidOperationException("Loja da compra não informada.");
        var entries = ChangeTracker.Entries().Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        if (entries.Any(Entry => Entry.Entity is not (QueueEntry or QueueSession or DropReservation or DropReservationLine or
            DropCatalogVariant or DropProduct or DropOrder or DropOrderItem or DropTransaction) || Entry.State == EntityState.Deleted))
            throw new SecurityFailure(403, "DropPurchase.InvalidScope", "O fluxo de compra não altera outros agregados.");

        foreach (var entry in entries)
        {
            if (_commercialWrites.Purpose == "expiration")
            {
                var fields = entry.Entity switch
                {
                    QueueEntry => new[] { "QueueEntryStatusId", "ExpiredAt", "UpdatedAt" },
                    QueueSession => new[] { "QueueSessionStatusId", "UpdatedAt" },
                    DropReservation => new[] { "DropReservationStatusId", "CancelledAt", "UpdatedAt" },
                    DropOrder => new[] { "DropOrderStatusId", "UpdatedAt" },
                    DropCatalogVariant => new[] { "QuantityReserved", "UpdatedAt" },
                    _ => Array.Empty<string>()
                };

                if (entry.State != EntityState.Modified || entry.Properties.Any(Property => Property.IsModified && !fields.Contains(Property.Metadata.Name)))
                    throw new SecurityFailure(403, "DropPurchase.InvalidExpiration", "A expiração altera somente prazo, estado e reservas.");
            }

            if (entry.Entity is DropReservationLine && entry.State != EntityState.Added ||
                entry.Entity is DropOrderItem && entry.State != EntityState.Added)
                throw new SecurityFailure(403, "DropPurchase.ImmutableSnapshot", "Os snapshots da compra são imutáveis.");

            if (entry.Entity is DropCatalogVariant variant)
            {
                var allowed = _commercialWrites.Purpose == "expiration" ? new[] { "QuantityReserved", "UpdatedAt" } :
                    new[] { "QuantityReserved", "QuantityOnHand", "UpdatedAt" };

                if (entry.State != EntityState.Modified || entry.Properties.Any(Property => Property.IsModified && !allowed.Contains(Property.Metadata.Name)) ||
                    variant.QuantityReserved < 0 || variant.QuantityOnHand < variant.QuantityReserved ||
                    !await Set<DropCatalogProduct>().IgnoreQueryFilters().AnyAsync(Product => Product.Id == variant.CatalogProductId && Product.EnterpriseId == tenant, ct))
                    throw new SecurityFailure(403, "DropPurchase.InvalidStock", "Alteração de saldo inválida.");
            }

            if (_commercialWrites.Purpose == "expiration" && entry.State == EntityState.Added)
                throw new SecurityFailure(403, "DropPurchase.InvalidExpiration", "A expiração não cria compras.");

            if (entry.Entity is DropProduct && (entry.State != EntityState.Modified || entry.Properties.Any(Property => Property.IsModified &&
                Property.Metadata.Name is not ("UnitsSold" or "UpdatedAt"))))
                throw new SecurityFailure(403, "DropPurchase.InvalidParticipation", "A compra não altera os termos da participação.");

            long? eventId = entry.Entity switch
            {
                QueueEntry queue => queue.DropEventId,
                QueueSession session => await QueueEntries.IgnoreQueryFilters().Where(Queue => Queue.Id == session.QueueEntryId).Select(Queue => (long?)Queue.DropEventId).SingleAsync(ct),
                DropReservation reservation => reservation.DropEventId,
                DropOrder order => order.DropEventId,
                _ => null
            };

            if (eventId.HasValue && !await DropEvents.IgnoreQueryFilters().AnyAsync(Event => Event.Id == eventId && Event.EnterpriseId == tenant, ct))
                throw new SecurityFailure(403, "DropPurchase.TenantMismatch", "Evento de outra loja.");

            if (entry.Entity is DropReservationLine line && !await DropReservations.IgnoreQueryFilters().AnyAsync(Reservation =>
                Reservation.Id == line.ReservationId && Reservation.DropEvent.EnterpriseId == tenant &&
                Reservation.DropProduct.CatalogProductId == line.Variant.CatalogProductId, ct) &&
                !entries.Any(Entry => Entry.Entity is DropReservation added && Entry.State == EntityState.Added && added.Lines.Contains(line)))
                throw new SecurityFailure(403, "DropPurchase.TenantMismatch", "Linha de reserva de outro produto ou loja.");

            if (entry.Entity is DropOrderItem item && !await DropOrders.IgnoreQueryFilters().AnyAsync(Order =>
                Order.Id == item.DropOrderId && Order.DropEvent.EnterpriseId == tenant &&
                Order.DropReservation.DropProductId == item.DropProductId, ct))
                throw new SecurityFailure(403, "DropPurchase.TenantMismatch", "Item de outro pedido ou loja.");
        }

        if (entries.Any(Entry => Entry.State == EntityState.Added && Entry.Entity is QueueEntry or QueueSession or DropReservation))
        {
            var rights = await _commercialReader.RightsAsync(tenant, ct);
            var applied = await Set<DropContractState>().IgnoreQueryFilters().AsNoTracking().SingleAsync(State => State.EnterpriseId == tenant, ct);

            if (!rights.AllowsDrop || applied.Revision != rights.ContractRevision)
                throw new SecurityFailure(409, "DropPurchase.ContractChanged", "O contrato mudou durante a admissão ou reserva.");
        }
    }
}
