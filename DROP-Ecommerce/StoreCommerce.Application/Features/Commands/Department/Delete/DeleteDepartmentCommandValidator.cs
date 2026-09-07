using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(d => d.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
