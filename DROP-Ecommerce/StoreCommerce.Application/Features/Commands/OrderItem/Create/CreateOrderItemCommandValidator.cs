using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
{
    public CreateOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(orderItem => orderItem)
            .Must(orderItem => orderItem.productId.HasValue != orderItem.serviceId.HasValue)
            .WithName("productId")
            .WithMessage("Informe apenas um entre productId e serviceId.");

        RuleFor(orderItem => orderItem.productId)
            .GreaterThan(0).When(orderItem => orderItem.productId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(orderItem => orderItem.serviceId)
            .GreaterThan(0).When(orderItem => orderItem.serviceId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(orderItem => orderItem.quantity)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(Item => Item.productVariantId).GreaterThan(0).When(Item => Item.productVariantId.HasValue);
        RuleFor(Item => Item.productId).NotNull().When(Item => Item.productVariantId.HasValue);
    }
}
