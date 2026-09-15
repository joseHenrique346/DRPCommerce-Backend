using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class EnterpriseValidator : AbstractValidator<Enterprise>
{
    public EnterpriseValidator()
    {
        RuleFor(e => e.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(e => e.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(e => e.UpdatedAt)
            .NotEqual(default(DateTime)).When(e => e.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(e => e.TradeName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(e => e.LegalName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(250).WithMessage("{PropertyName} não pode ter mais de 250 caracteres.");

        RuleFor(e => e.Email)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.Phone)
            .NotNull().WithMessage("{PropertyName} não pode ser nulo.");

        RuleFor(e => e.AddressLine)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(300).WithMessage("{PropertyName} não pode ter mais de 300 caracteres.");

        RuleFor(e => e.City)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(e => e.State)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(e => e.ZipCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(e => e.Country)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");
    }
}
