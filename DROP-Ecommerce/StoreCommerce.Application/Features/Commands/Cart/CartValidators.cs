using FluentValidation;
using StoreCommerce.Domain.Entity;
using CartEntity = StoreCommerce.Domain.Entity.Cart;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator()
    {
        RuleFor(query => query.CouponCode).MaximumLength(Coupon.CodeMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

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

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(command => command.ItemId).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Quantity).InclusiveBetween(1, CartEntity.MaxQuantityPerItem)
            .WithMessage("{PropertyName} deve estar entre {From} e {To}. Para tirar o item, remova-o do carrinho.");
    }
}

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

public sealed class SetCartSelectionCommandValidator : AbstractValidator<SetCartSelectionCommand>
{
    public SetCartSelectionCommandValidator()
    {
        RuleFor(command => command.ItemIds)
            .Must(ids => ids is null || ids.Count <= CartEntity.MaxDistinctItems)
            .WithMessage($"Informe no máximo {CartEntity.MaxDistinctItems} itens.");
        RuleForEach(command => command.ItemIds).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}

public sealed class AwaitCartItemStockCommandValidator : AbstractValidator<AwaitCartItemStockCommand>
{
    public AwaitCartItemStockCommandValidator()
    {
        RuleFor(command => command.ItemId).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
