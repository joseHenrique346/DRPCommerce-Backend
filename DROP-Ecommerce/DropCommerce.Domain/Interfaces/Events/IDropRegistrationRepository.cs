using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Domain.Interfaces.Events;

public interface IDropRegistrationRepository : IRepository<DropRegistration>
{
    Task<IEnumerable<DropRegistration>> GetByEventAsync(long dropEventId, CancellationToken cancellationToken = default);
    Task<DropRegistration?> GetByCustomerAndEventAsync(long customerId, long dropEventId, CancellationToken cancellationToken = default);
}
