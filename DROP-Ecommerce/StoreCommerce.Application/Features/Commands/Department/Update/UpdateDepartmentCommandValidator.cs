using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(d => d.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(d => d.description)
            .MaximumLength(1000).WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");
    }
}
