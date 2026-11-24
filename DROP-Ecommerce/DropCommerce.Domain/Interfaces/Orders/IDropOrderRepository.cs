using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces.Orders;

public interface IDropOrderRepository : IRepository<DropOrder>
{
    Task<IEnumerable<DropOrder>> GetByCustomerAsync(long customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DropOrder>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default);
}
