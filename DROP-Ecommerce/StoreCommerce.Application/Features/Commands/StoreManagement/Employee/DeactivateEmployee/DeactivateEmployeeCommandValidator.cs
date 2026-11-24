using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeactivateEmployeeCommandValidator : AbstractValidator<DeactivateEmployeeCommand>
{
    public DeactivateEmployeeCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
