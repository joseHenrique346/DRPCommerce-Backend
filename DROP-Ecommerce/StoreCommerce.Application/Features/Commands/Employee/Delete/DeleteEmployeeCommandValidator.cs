using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(e => e.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}