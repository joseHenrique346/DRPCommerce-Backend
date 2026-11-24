using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetMyAddressesQueryHandler(
    ICustomerAddressRepository addresses,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider)
    : WorkflowRequestHandler<GetMyAddressesQuery, List<CustomerAddressResponse>>
{
    protected override async Task<List<CustomerAddressResponse>> ExecuteAsync(GetMyAddressesQuery command, CancellationToken ct)
    {
        var (enterpriseId, customerId) = (tenantProvider.GetEnterpriseId(), actor.CustomerId);

        return (await addresses.ListForCustomerAsync(enterpriseId, customerId, tracking: false, ct)).Select(CustomerAddressResponse.FromEntity).ToList();
    }
}
