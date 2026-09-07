using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateOrderItemCommandValidator : AbstractValidator<UpdateOrderItemCommand>
{
    public UpdateOrderItemCommandValidator()
    {
        RuleFor(oi => oi.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(oi => oi.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(oi => oi.productId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero quando fornecido.")
            .When(oi => oi.productId.HasValue);

        RuleFor(oi => oi.serviceId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero quando fornecido.")
            .When(oi => oi.serviceId.HasValue);

        RuleFor(oi => oi.itemName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(oi => oi.sku)
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(oi => oi.quantity)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(oi => oi.unitPrice)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(oi => oi.discountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(oi => oi.totalPrice)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
