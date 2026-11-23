using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Interfaces.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StockReservationExpirationWriter(
    IInventoryRepository repository)
{
    public async Task ExpireAsync(long enterpriseId, DateTime now, CancellationToken ct)
    {
        var expired = await repository.GetExpiredAsync(enterpriseId, now, ct);

        foreach (var reservation in expired)
        {
            foreach (var line in reservation.Lines)
            {
                var variant = line.ProductVariant;
                variant.ReleaseStock(line.Quantity);
                repository.AddMovement(StockMovement.Create(variant, reservation, StockMovementType.Expiration,
                    reservation.IdempotencyKey, 0, -line.Quantity, "Expiração de reserva", null, now));
            }

            reservation.Complete(StockReservationStatus.Expired, now);
        }
    }
}
