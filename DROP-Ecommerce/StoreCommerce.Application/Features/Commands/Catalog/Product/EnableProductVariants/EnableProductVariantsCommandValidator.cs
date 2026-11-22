using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class EnableProductVariantsCommandValidator : AbstractValidator<EnableProductVariantsCommand>
{
    public EnableProductVariantsCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}
