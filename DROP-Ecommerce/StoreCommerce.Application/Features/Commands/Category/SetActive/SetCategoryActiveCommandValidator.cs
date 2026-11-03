using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetCategoryActiveCommandValidator : AbstractValidator<SetCategoryActiveCommand>
{
    public SetCategoryActiveCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}
