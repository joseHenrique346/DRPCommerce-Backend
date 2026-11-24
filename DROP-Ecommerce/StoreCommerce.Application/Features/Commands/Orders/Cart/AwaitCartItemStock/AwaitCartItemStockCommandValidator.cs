using FluentValidation;
using StoreCommerce.Domain.Entity;
using CartEntity = StoreCommerce.Domain.Entity.Cart;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AwaitCartItemStockCommandValidator : AbstractValidator<AwaitCartItemStockCommand>
{
    public AwaitCartItemStockCommandValidator()
    {
        RuleFor(command => command.ItemId).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
