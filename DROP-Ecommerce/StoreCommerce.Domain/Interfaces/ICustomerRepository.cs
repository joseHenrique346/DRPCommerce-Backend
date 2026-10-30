using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IEnumerable<Customer>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task LockAsync(long id, CancellationToken cancellationToken);
    Task<(List<Models.CustomerSearchRow> Items, int TotalCount)> SearchAsync(Models.CustomerSearchCriteria criteria, CancellationToken cancellationToken);
}
