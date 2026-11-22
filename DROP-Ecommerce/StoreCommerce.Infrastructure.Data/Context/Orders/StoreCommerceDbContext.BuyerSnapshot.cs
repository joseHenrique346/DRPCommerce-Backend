using StoreCommerce.Domain.Enums.Orders;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private async Task CaptureOrderBuyerSnapshotsAsync(CancellationToken ct)
    {
        var addedOrders = ChangeTracker.Entries<Order>().Where(Entry => Entry.State == EntityState.Added)
            .Select(Entry => Entry.Entity).OrderBy(Order => Order.CustomerId).ToList();

        foreach (var enterpriseId in addedOrders.Select(Order => Order.EnterpriseId).Distinct().OrderBy(Id => Id))
            await Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {enterpriseId} FOR UPDATE", ct);

        foreach (var order in addedOrders)
        {
            // Compartilhado com a exclusão da conta: ou o pedido nasce primeiro e bloqueia a exclusão,
            // ou o cadastro já encerrado impede que uma requisição antiga crie uma venda depois.
            await Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM cliente WHERE id = {order.CustomerId} FOR UPDATE", ct);
            var customer = await Customers.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(Customer => Customer.Id == order.CustomerId && Customer.EnterpriseId == order.EnterpriseId &&
                    Customer.IsActive && !Customer.IsDeleted && Customer.PersonalDataErasedAt == null, ct)
                ?? throw new SecurityFailure(409, "Order.BuyerUnavailable", "O comprador não está disponível para um novo pedido.");
            order.CaptureBuyerSnapshot(customer, DateTime.UtcNow, BuyerSnapshotSource.AtOrderCreation);
        }

        var snapshotProperties = new[]
        {
            nameof(Order.CustomerId), nameof(Order.BuyerName), nameof(Order.BuyerEmail), nameof(Order.BuyerPhone),
            nameof(Order.BuyerSnapshotCapturedAt), nameof(Order.BuyerSnapshotSource)
        };

        foreach (var entry in ChangeTracker.Entries<Order>().Where(Entry => Entry.State == EntityState.Modified))
        {
            if (snapshotProperties.Any(Name => entry.Property(Name).IsModified &&
                !Equals(entry.Property(Name).OriginalValue, entry.Property(Name).CurrentValue)))
                throw new SecurityFailure(409, "Order.ImmutableBuyerSnapshot", "A identificação histórica do comprador não pode ser reescrita.");
        }
    }
}
