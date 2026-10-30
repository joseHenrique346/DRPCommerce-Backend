using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    /// <summary>
    /// Checkout e ciclo de vida do pedido gravam, na mesma transação, pedido, pagamento, estoque, cupom e carrinho.
    /// Com o escopo aberto só essas entidades da loja do escopo podem mudar, e cada uma apenas do jeito que o fluxo exige.
    /// </summary>
    private bool GuardOrderWorkflowWrites(List<EntityEntry> changes)
    {
        if (_orderWorkflow.EnterpriseId is not long tenant)
            return false;

        var customer = _orderWorkflow.CustomerId;
        var orders = ChangeTracker.Entries<Order>()
            .Where(entry => entry.Entity.EnterpriseId == tenant && (customer == null || entry.Entity.CustomerId == customer))
            .Select(entry => entry.Entity)
            .ToList();

        bool OwnsOrder(Order? order, long orderId) =>
            orders.Any(item => ReferenceEquals(item, order) || (orderId > 0 && item.Id == orderId));

        foreach (var entry in changes)
        {
            if (entry.Entity is ITenantEntity scoped && scoped.EnterpriseId != tenant)
                throw Invalid();

            if (entry.State == EntityState.Deleted && entry.Entity is not CartItem)
                throw Invalid();

            var allowed = entry.Entity switch
            {
                Order order => orders.Contains(order) &&
                    !(entry.State == EntityState.Modified && entry.Property(nameof(Order.CustomerId)).IsModified),
                OrderItem item => entry.State == EntityState.Added && OwnsOrder(item.Order, item.OrderId) ||
                    entry.State == EntityState.Modified && OwnsOrder(item.Order, item.OrderId),
                Shipment shipment => (entry.State == EntityState.Added || entry.State == EntityState.Modified) && OwnsOrder(shipment.Order, shipment.OrderId),
                OrderStatusHistory history => entry.State == EntityState.Added && OwnsOrder(history.Order, history.OrderId),
                Transaction transaction => OwnsOrder(transaction.Order, transaction.OrderId),
                CouponRedemption redemption => OwnsOrder(redemption.Order, redemption.OrderId),
                Coupon => entry.State == EntityState.Modified && OnlyModified(entry, nameof(Coupon.UsedCount)),
                StockReservation => true,
                StockReservationLine or StockMovement => entry.State == EntityState.Added,
                ProductVariant => entry.State == EntityState.Modified && OnlyModified(entry,
                    nameof(ProductVariant.StockQuantity), nameof(ProductVariant.ReservedQuantity), nameof(ProductVariant.Version)),
                CartItem item => entry.State == EntityState.Deleted &&
                    (item.Cart ?? ChangeTracker.Entries<Cart>().Select(cart => cart.Entity).FirstOrDefault(cart => cart.Id == item.CartId)) is { } owner &&
                    owner.EnterpriseId == tenant && (customer == null || owner.CustomerId == customer),
                _ => false
            };

            if (!allowed)
                throw Invalid();
        }

        return true;

        static SecurityFailure Invalid() => new(403, "Order.InvalidWorkflowScope", "Gravação fora do escopo do fluxo de pedido.");
    }

    private static bool OnlyModified(EntityEntry entry, params string[] properties) =>
        entry.Properties.All(property => !property.IsModified ||
            properties.Contains(property.Metadata.Name) || property.Metadata.Name == nameof(BaseEntity.UpdatedAt));
}
