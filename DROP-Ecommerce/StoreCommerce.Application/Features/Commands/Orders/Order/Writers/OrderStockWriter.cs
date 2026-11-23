using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Interfaces.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class OrderStockWriter(
    IInventoryRepository inventory)
{
    public void CommitStock(Order order, StoreSettings settings, DateTime now)
    {
        var reservation = order.StockReservation;

        // Uma baixa já concluída, inclusive por versões anteriores, não pode ser repetida na confirmação do pagamento.
        if (reservation?.Status == StockReservationStatus.Committed)
            return;

        if (reservation is null || reservation.Status != StockReservationStatus.Reserved)
        {
            // Pedido do backoffice (sem reserva) ou reserva expirada: reserva agora com o saldo atual e baixa em seguida.
            var lines = order.ListOrderItem.Where(item => item.ProductVariant is { } variant && OrderStockPolicy.ControlsStock(variant)).ToList();

            if (lines.Count == 0)
                return;

            reservation = StockReservation.Create(order.EnterpriseId, $"order-{order.Id}-confirm", $"Pedido {order.Id}", now);

            foreach (var item in lines)
            {
                var variant = item.ProductVariant!;

                if (!settings.AllowBackorder && variant.AvailableQuantity < item.Quantity)
                    throw new SecurityFailure(409, "Order.StockUnavailable", $"Estoque insuficiente para o SKU {variant.SKU}.");

                reservation.AddLine(variant, item.Quantity, settings.AllowBackorder);
                variant.ReserveStock(item.Quantity, settings.AllowBackorder);
                inventory.AddMovement(StockMovement.Create(variant, reservation, StockMovementType.Reservation, reservation.IdempotencyKey,
                    0, item.Quantity, "Reserva na confirmação do pedido", null, now));
            }

            inventory.AddReservation(reservation);
            order.AttachStockReservation(reservation);
        }

        foreach (var line in reservation.Lines)
        {
            line.ProductVariant.CommitStock(line.Quantity);
            inventory.AddMovement(StockMovement.Create(line.ProductVariant, reservation, StockMovementType.Commit, reservation.IdempotencyKey,
                -line.Quantity, -line.Quantity, $"Baixa do pedido {order.Id}", null, now));
        }

        reservation.Complete(StockReservationStatus.Committed, now);
    }

    public void ReturnStock(Order order, DateTime now)
    {
        var reservation = order.StockReservation;

        if (reservation is null)
            return;

        if (reservation.Status == StockReservationStatus.Reserved)
        {
            foreach (var line in reservation.Lines)
            {
                line.ProductVariant.ReleaseStock(line.Quantity);
                inventory.AddMovement(StockMovement.Create(line.ProductVariant, reservation, StockMovementType.Release, reservation.IdempotencyKey,
                    0, -line.Quantity, $"Cancelamento do pedido {order.Id}", null, now));
            }

            reservation.Complete(StockReservationStatus.Released, now);
        }
        else if (reservation.Status == StockReservationStatus.Committed)
        {
            foreach (var line in reservation.Lines)
            {
                line.ProductVariant.AdjustStock(line.Quantity, allowBackorder: true);
                inventory.AddMovement(StockMovement.Create(line.ProductVariant, reservation, StockMovementType.Return, $"order-{order.Id}-return",
                    line.Quantity, 0, $"Devolução do cancelamento do pedido {order.Id}", null, now));
            }
        }
    }
}
