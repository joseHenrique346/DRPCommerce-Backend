using FluentValidation;
using StoreCommerce.Domain.Entity;
using CartEntity = StoreCommerce.Domain.Entity.Cart;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(command => command.ItemId).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Quantity).InclusiveBetween(1, CartEntity.MaxQuantityPerItem)
            .WithMessage("{PropertyName} deve estar entre {From} e {To}. Para tirar o item, remova-o do carrinho.");
    }
}
