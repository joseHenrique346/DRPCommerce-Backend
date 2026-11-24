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

public sealed class AddMyAddressCommandValidator : AbstractValidator<AddMyAddressCommand>
{
    public AddMyAddressCommandValidator() => RuleFor(command => command.Request).NotNull().SetValidator(new CustomerAddressRequestValidator());
}
