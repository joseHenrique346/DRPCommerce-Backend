using FluentValidation;
using StoreCommerce.Domain.Entity;
using CartEntity = StoreCommerce.Domain.Entity.Cart;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.ProductVariantId).GreaterThan(0).When(command => command.ProductVariantId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");
        RuleFor(command => command.Quantity).InclusiveBetween(1, CartEntity.MaxQuantityPerItem)
            .WithMessage("{PropertyName} deve estar entre {From} e {To}.");
    }
}
