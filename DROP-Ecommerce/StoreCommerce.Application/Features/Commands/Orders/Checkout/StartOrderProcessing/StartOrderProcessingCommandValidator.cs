using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartOrderProcessingCommandValidator
    : AbstractValidator<StartOrderProcessingCommand>
{
    public StartOrderProcessingCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
