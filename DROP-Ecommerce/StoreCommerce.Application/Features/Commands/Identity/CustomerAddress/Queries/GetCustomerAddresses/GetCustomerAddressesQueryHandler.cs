using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;
using FluentValidation;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCustomerAddressesQueryHandler(ICustomerAddressRepository addresses, IRepository<Customer> customers, ITenantProvider tenant)
    : WorkflowRequestHandler<GetCustomerAddressesQuery, List<CustomerAddressResponse>>
{
    protected override async Task<List<CustomerAddressResponse>> ExecuteAsync(GetCustomerAddressesQuery request, CancellationToken ct)
    {
        if (!await customers.ExistsAsync(customer => customer.Id == request.CustomerId, ct))
            throw new SecurityFailure(404, "Customer.NotFound", "Cliente não encontrado.");

        return (await addresses.ListForCustomerAsync(tenant.GetEnterpriseId(), request.CustomerId, tracking: false, ct))
            .Select(CustomerAddressResponse.FromEntity).ToList();
    }
}
