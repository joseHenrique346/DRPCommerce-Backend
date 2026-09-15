using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
{
    public CreateOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(orderItem => orderItem.productId)
            .GreaterThan(0).When(orderItem => orderItem.productId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(orderItem => orderItem.serviceId)
            .GreaterThan(0).When(orderItem => orderItem.serviceId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(orderItem => orderItem.itemName)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(200).WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(orderItem => orderItem.sku)
            .MaximumLength(100).When(orderItem => !string.IsNullOrWhiteSpace(orderItem.sku))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(orderItem => orderItem.quantity)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(orderItem => orderItem.unitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(orderItem => orderItem.discountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(orderItem => orderItem.totalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");
    }
}