using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateProductDimensionsCommandValidator : AbstractValidator<UpdateProductDimensionsCommand>
{
    public UpdateProductDimensionsCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.Weight).GreaterThanOrEqualTo(0);
        RuleFor(Command => Command.Height).GreaterThanOrEqualTo(0);
        RuleFor(Command => Command.Width).GreaterThanOrEqualTo(0);
        RuleFor(Command => Command.Length).GreaterThanOrEqualTo(0);
    }
}
