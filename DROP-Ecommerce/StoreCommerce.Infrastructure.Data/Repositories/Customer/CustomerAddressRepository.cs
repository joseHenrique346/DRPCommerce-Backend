using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class CustomerAddressRepository(StoreCommerceDbContext context) : ICustomerAddressRepository
{
    public Task<List<CustomerAddress>> ListForCustomerAsync(long enterpriseId, long customerId, bool tracking, CancellationToken cancellationToken)
    {
        var query = context.CustomerAddresses
            .Where(address => address.EnterpriseId == enterpriseId && address.CustomerId == customerId)
            .OrderByDescending(address => address.IsDefault).ThenByDescending(address => address.Id);

        return (tracking ? query : query.AsNoTracking()).ToListAsync(cancellationToken);
    }

    public void Add(CustomerAddress address) => context.CustomerAddresses.Add(address);

    public void Remove(CustomerAddress address) => context.CustomerAddresses.Remove(address);
}
