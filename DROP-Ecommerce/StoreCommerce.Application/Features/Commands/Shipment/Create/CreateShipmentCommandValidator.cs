using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(s => s.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.supplierId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero quando fornecido.")
            .When(s => s.supplierId.HasValue);

        RuleFor(s => s.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.carrierName)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.trackingCode)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(s => s.shippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(s => s.addressLine)
            .MaximumLength(500).WithMessage("{PropertyName} não pode ter mais de 500 caracteres.");

        RuleFor(s => s.city)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.state)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(s => s.zipCode)
            .MaximumLength(20).WithMessage("{PropertyName} não pode ter mais de 20 caracteres.");

        RuleFor(s => s.country)
            .MaximumLength(100).WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(s => s.shippedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.")
            .When(s => s.shippedAt.HasValue);

        RuleFor(s => s.deliveredAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.")
            .When(s => s.deliveredAt.HasValue);
    }
}
