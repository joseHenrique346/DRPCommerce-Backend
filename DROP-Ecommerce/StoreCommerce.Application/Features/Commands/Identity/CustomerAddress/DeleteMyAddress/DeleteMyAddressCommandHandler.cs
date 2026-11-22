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

public sealed class DeleteMyAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<DeleteMyAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(DeleteMyAddressCommand request, CancellationToken ct) =>
        service.DeleteAsync(request.Id, ct);
}
