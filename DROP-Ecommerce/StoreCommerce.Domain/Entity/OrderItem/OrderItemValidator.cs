using FluentValidation;

namespace StoreCommerce.Domain.Entity;

public class OrderItemValidator : AbstractValidator<OrderItem>
{
    public OrderItemValidator()
    {
        RuleFor(oi => oi.Id)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(oi => oi.CreatedAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(oi => oi.UpdatedAt)
            .NotEqual(default(DateTime)).When(oi => oi.UpdatedAt.HasValue)
            .WithMessage("{PropertyName} deve ser uma data válida quando fornecido.");

        RuleFor(oi => oi.OrderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(oi => oi.ProductId)
            .GreaterThan(0).When(oi => oi.ProductId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(oi => oi.ServiceId)
            .GreaterThan(0).When(oi => oi.ServiceId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(oi => oi.ItemName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(oi => oi.SKU)
            .MaximumLength(100).When(oi => !string.IsNullOrWhiteSpace(oi.SKU))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(oi => oi.Quantity)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(oi => oi.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(oi => oi.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(oi => oi.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");
    }
}
