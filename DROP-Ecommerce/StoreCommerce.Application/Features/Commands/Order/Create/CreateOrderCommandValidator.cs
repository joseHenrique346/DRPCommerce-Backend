using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(order => order.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(order => order.customerId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(order => order.couponId)
            .GreaterThan(0).When(order => order.couponId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(order => order.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(order => order.paymentStatusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(order => order.subTotal)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(order => order.discountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(order => order.shippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(order => order.taxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(order => order.totalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(order => order.shippingAddressLine)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(300).WithMessage("{PropertyName} não pode ter mais de 300 caracteres.");

        RuleFor(order => order.shippingCity)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(150).WithMessage("{PropertyName} não pode ter mais de 150 caracteres.");

        RuleFor(order => order.shippingState)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(order => order.shippingZipCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(order => order.notes)
            .MaximumLength(1000).When(order => !string.IsNullOrWhiteSpace(order.notes))
            .WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");
    }
}