using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class DepartmentValidator : AbstractValidator<Department>
{
    public DepartmentValidator()
    {
        RuleFor(d => d.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(d => d.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(d => d.UpdatedAt)
            .NotEqual(default(DateTime)).When(d => d.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(d => d.Description)
            .MaximumLength(500).When(d => !string.IsNullOrWhiteSpace(d.Description))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");
    }
}
