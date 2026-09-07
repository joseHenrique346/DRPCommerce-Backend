using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(t => t.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.customerId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.methodId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.amount)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(t => t.fee)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(t => t.gatewayReference)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(t => t.gatewayProvider)
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(t => t.gatewayPayload)
            .MaximumLength(5000).WithMessage("{PropertyName} não pode ter mais de 5000 caracteres.");

        RuleFor(t => t.paidAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.")
            .When(t => t.paidAt.HasValue);

        RuleFor(t => t.refundedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("{PropertyName} não pode estar no futuro.")
            .When(t => t.refundedAt.HasValue);
    }
}
