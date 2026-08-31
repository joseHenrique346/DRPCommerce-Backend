using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class TransactionValidator : AbstractValidator<Transaction>
{
    public TransactionValidator()
    {
        RuleFor(t => t.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(t => t.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(t => t.UpdatedAt)
            .NotEqual(default(DateTime)).When(t => t.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(t => t.OrderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.CustomerId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.TypeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.MethodId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.StatusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.Amount)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.Fee)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(t => t.GatewayReference)
            .MaximumLength(200).When(t => !string.IsNullOrWhiteSpace(t.GatewayReference))
            .WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(t => t.GatewayProvider)
            .MaximumLength(100).When(t => !string.IsNullOrWhiteSpace(t.GatewayProvider))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(t => t.GatewayPayload)
            .MaximumLength(5000).When(t => !string.IsNullOrWhiteSpace(t.GatewayPayload))
            .WithMessage("{PropertyName} não pode ter mais de 5000 caracteres.");

        RuleFor(t => t.PaidAt)
            .NotEqual(default(DateTime)).When(t => t.PaidAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(t => t.RefundedAt)
            .NotEqual(default(DateTime)).When(t => t.RefundedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");
    }
}
