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

public sealed class AddMyAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<AddMyAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(AddMyAddressCommand request, CancellationToken ct) => service.AddAsync(request.Request, ct);
}
