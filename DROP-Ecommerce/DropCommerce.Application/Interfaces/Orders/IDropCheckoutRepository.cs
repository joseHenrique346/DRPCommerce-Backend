using DropCommerce.Application.Contracts;
using DropCommerce.Application.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Interfaces.Orders;

public interface IDropCheckoutRepository : IDropCommercePersistence
{
    Task<DropReservation?> FindReservationAsync(long reservationId, long customerId, long tenant, CancellationToken ct);
    Task<DropOrder?> FindReservationOrderAsync(long reservationId, long customerId, long tenant, CancellationToken ct);
    Task<DropOrder> GetOrderWithReservationAsync(long orderId, long tenant, CancellationToken ct);
    Task<long?> FindEventIdAsync(long orderId, long tenant, CancellationToken ct);
    Task<DropOrder?> FindOrderForReceiptAsync(long orderId, long tenant, CancellationToken ct);
    Task<DropProduct> GetParticipationAsync(long participationId, long tenant, CancellationToken ct);
    Task<DropOrder?> FindCustomerOrderAsync(long orderId, long customer, long tenant, CancellationToken ct);
    Task<DropOrder?> FindOrderAsync(long orderId, long tenant, CancellationToken ct);
    Task<DropOrder> ReadOrderAsync(long orderId, long tenant, CancellationToken ct);
    Task<DropOrder> GetOrderAsync(long orderId, long tenant, CancellationToken ct);
}
