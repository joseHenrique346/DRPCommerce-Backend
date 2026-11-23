using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Interfaces.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StockReservationCompletionWriter(
    IInventoryRepository repository)
{
    public void Complete(StockReservation reservation, bool commit, long userId, DateTime now)
    {
        var expected = commit ? StockReservationStatus.Committed : StockReservationStatus.Released;
        if (reservation.Status == StockReservationStatus.Reserved)
        {
            foreach (var line in reservation.Lines)
            {
                var variant = line.ProductVariant;

                if (commit && (long)variant.StockQuantity - line.Quantity < int.MinValue)
                    throw new SecurityFailure(409, "Inventory.QuantityOverflow", "O SKU atingiu o limite de saldo negativo.");

                if (commit)
                    variant.CommitStock(line.Quantity);
                else
                    variant.ReleaseStock(line.Quantity);

                repository.AddMovement(StockMovement.Create(variant, reservation, commit ? StockMovementType.Commit : StockMovementType.Release,
                    reservation.IdempotencyKey, commit ? -line.Quantity : 0, -line.Quantity,
                    commit ? "Baixa de reserva" : "Liberação de reserva", userId, now));
            }

            reservation.Complete(expected, now);
        }
    }
}
