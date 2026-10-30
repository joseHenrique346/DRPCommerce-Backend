using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces;

public interface ICustomerAddressRepository
{
    Task<List<CustomerAddress>> ListForCustomerAsync(long enterpriseId, long customerId, bool tracking, CancellationToken cancellationToken);
    void Add(CustomerAddress address);
    void Remove(CustomerAddress address);
}
