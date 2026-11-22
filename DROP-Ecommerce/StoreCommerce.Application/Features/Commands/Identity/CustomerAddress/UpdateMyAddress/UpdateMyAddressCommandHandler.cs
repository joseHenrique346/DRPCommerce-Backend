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

public sealed class UpdateMyAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<UpdateMyAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(UpdateMyAddressCommand request, CancellationToken ct) =>
        service.UpdateAsync(request.Id, request.Request, ct);
}
