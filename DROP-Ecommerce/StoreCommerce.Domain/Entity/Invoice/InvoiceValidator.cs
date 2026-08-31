using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class InvoiceValidator : AbstractValidator<Invoice>
{
    public InvoiceValidator()
    {
        RuleFor(i => i.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(i => i.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(i => i.UpdatedAt)
            .NotEqual(default(DateTime)).When(i => i.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(i => i.OrderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(i => i.CustomerId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(i => i.EnterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(i => i.Number)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(i => i.Series)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(i => i.AccessKey)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(i => i.TypeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(i => i.StatusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(i => i.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(i => i.FileUrl)
            .MaximumLength(500).When(i => !string.IsNullOrWhiteSpace(i.FileUrl))
            .WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(i => i.IssuedAt)
            .NotEqual(default(DateTime)).When(i => i.IssuedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");
    }
}
