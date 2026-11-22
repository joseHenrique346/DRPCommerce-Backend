using DropCommerce.Application.Interfaces.Orders;
using Microsoft.EntityFrameworkCore;
using DropCommerce.Domain.Entity;
using DropCommerce.Application.Contracts;
using DropCommerce.Infrastructure.Data.Context;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropCheckoutRepository(DropCommerceDbContext db) : DropCommercePersistence(db), IDropCheckoutRepository
{
    public Task<DropReservation?> FindReservationAsync(long reservationId, long customerId, long tenant, CancellationToken ct)
    {
        return Context.DropReservations.IgnoreQueryFilters().Include(Reservation => Reservation.Lines)
                .SingleOrDefaultAsync(Reservation => Reservation.Id == reservationId && Reservation.CustomerId == customerId &&
                    Reservation.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropOrder?> FindReservationOrderAsync(long reservationId, long customerId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().SingleOrDefaultAsync(Order => Order.DropReservationId == reservationId &&
                Order.CustomerId == customerId && Order.DropEvent.EnterpriseId == tenant && Order.PaymentOperationId != null, ct);
    }

    public Task<DropOrder> GetOrderWithReservationAsync(long orderId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().AsNoTracking().Include(Order => Order.DropReservation)
            .SingleAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<long?> FindEventIdAsync(long orderId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().Where(Order => Order.Id == orderId &&
            Order.DropEvent.EnterpriseId == tenant).Select(Order => (long?)Order.DropEventId).SingleOrDefaultAsync(ct);
    }

    public Task<DropOrder?> FindOrderForReceiptAsync(long orderId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().Include(Order => Order.DropReservation)
            .ThenInclude(Reservation => Reservation.Lines).ThenInclude(Line => Line.Variant)
            .SingleOrDefaultAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropProduct> GetParticipationAsync(long participationId, long tenant, CancellationToken ct)
    {
        return Context.DropProducts.IgnoreQueryFilters().SingleAsync(Product => Product.Id == participationId &&
                Product.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropOrder?> FindCustomerOrderAsync(long orderId, long customer, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().AsNoTracking().Include(Order => Order.ListDropOrderItem)
            .SingleOrDefaultAsync(Order => Order.Id == orderId && Order.CustomerId == customer && Order.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropOrder?> FindOrderAsync(long orderId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().SingleOrDefaultAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropOrder> ReadOrderAsync(long orderId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);
    }

    public Task<DropOrder> GetOrderAsync(long orderId, long tenant, CancellationToken ct)
    {
        return Context.DropOrders.IgnoreQueryFilters().SingleAsync(Order => Order.Id == orderId && Order.DropEvent.EnterpriseId == tenant, ct);
    }
}
