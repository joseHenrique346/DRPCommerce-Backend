using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
    {
        RuleFor(d => d.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(d => d.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(d => d.UpdatedAt)
            .NotEqual(default(DateTime)).When(d => d.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(d => d.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.ReferenceId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.ReferenceType)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(d => d.TypeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.Number)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(d => d.FileUrl)
            .MaximumLength(500).When(d => !string.IsNullOrWhiteSpace(d.FileUrl))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(d => d.StatusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(d => d.IssuedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(d => d.ExpiresAt)
            .NotEqual(default(DateTime)).When(d => d.ExpiresAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");
    }
}
