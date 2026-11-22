using FluentValidation;
using StoreCommerce.Application.Contracts.Product;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductVariantPricingCommandValidator : AbstractValidator<UpdateProductVariantPricingCommand>
{
    public UpdateProductVariantPricingCommandValidator()
    {
        RuleFor(Command => Command.ProductId).GreaterThan(0);
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Request).NotNull();
    }
}
