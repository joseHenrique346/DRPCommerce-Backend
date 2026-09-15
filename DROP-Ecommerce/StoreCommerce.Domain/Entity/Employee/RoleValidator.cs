using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class RoleValidator : AbstractValidator<Role>
{
    public RoleValidator()
    {
        RuleFor(r => r.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(r => r.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(r => r.UpdatedAt)
            .NotEqual(default(DateTime)).When(r => r.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(r => r.Description)
            .MaximumLength(500).When(r => !string.IsNullOrWhiteSpace(r.Description))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");
    }
}
