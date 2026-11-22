using FluentValidation;
using StoreCommerce.Domain.Entity;
using CartEntity = StoreCommerce.Domain.Entity.Cart;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RemoveCartItemsCommandValidator : AbstractValidator<RemoveCartItemsCommand>
{
    public RemoveCartItemsCommandValidator()
    {
        RuleFor(command => command.ItemIds).NotEmpty().WithMessage("Informe ao menos um item.")
            .Must(ids => ids is null || ids.Count <= CartEntity.MaxDistinctItems)
            .WithMessage($"Informe no máximo {CartEntity.MaxDistinctItems} itens.");
        RuleForEach(command => command.ItemIds).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
