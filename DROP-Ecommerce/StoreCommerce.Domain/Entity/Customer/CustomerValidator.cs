using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(c => c.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(c => c.UpdatedAt)
            .NotEqual(default(DateTime)).When(c => c.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(c => c.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(c => c.FullName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(c => c.PasswordHash)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(c => c.AddressLine)
            .MaximumLength(300).When(c => !string.IsNullOrWhiteSpace(c.AddressLine))
            .WithMessage("{PropertyName} não pode ter mais de 300 caracteres.");

        RuleFor(c => c.City)
            .MaximumLength(150).When(c => !string.IsNullOrWhiteSpace(c.City))
            .WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(c => c.State)
            .MaximumLength(100).When(c => !string.IsNullOrWhiteSpace(c.State))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(c => c.ZipCode)
            .MaximumLength(20).When(c => !string.IsNullOrWhiteSpace(c.ZipCode))
            .WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(c => c.Country)
            .MaximumLength(100).When(c => !string.IsNullOrWhiteSpace(c.Country))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(c => c.Gender)
            .MaximumLength(50).When(c => !string.IsNullOrWhiteSpace(c.Gender))
            .WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(c => c.DateOfBirth)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");
    }
}
