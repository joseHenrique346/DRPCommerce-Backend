using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductPricingCommandValidator : AbstractValidator<UpdateProductPricingCommand>
{
    public UpdateProductPricingCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Price).GreaterThan(0);
        RuleFor(Command => Command.CostPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(Command => Command.Price);
    }
}
