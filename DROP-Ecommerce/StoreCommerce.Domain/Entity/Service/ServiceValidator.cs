using FluentValidation;

namespace StoreCommerce.Domain.Entity.Service;

public class ServiceValidator : AbstractValidator<Service>
{
    public ServiceValidator()
    {
        RuleFor(s => s.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(s => s.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(s => s.UpdatedAt)
            .NotEqual(default(DateTime)).When(s => s.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(s => s.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.CategoryId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(s => s.Description)
            .MaximumLength(2000).When(s => !string.IsNullOrWhiteSpace(s.Description))
            .WithMessage("{PropertyName} não pode ter mais de 2000 caracteres.");

        RuleFor(s => s.Price)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.DurationMinutes)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
