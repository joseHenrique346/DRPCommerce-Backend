using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(transaction => transaction.orderId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(transaction => transaction.customerId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(transaction => transaction.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(transaction => transaction.methodId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(transaction => transaction.statusId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(transaction => transaction.amount)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(transaction => transaction.fee)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(transaction => transaction.gatewayReference)
            .MaximumLength(200).When(transaction => !string.IsNullOrWhiteSpace(transaction.gatewayReference))
            .WithMessage("{PropertyName} não pode ter mais de 200 caracteres.");

        RuleFor(transaction => transaction.gatewayProvider)
            .MaximumLength(100).When(transaction => !string.IsNullOrWhiteSpace(transaction.gatewayProvider))
            .WithMessage("{PropertyName} não pode ter mais de 100 caracteres.");

        RuleFor(transaction => transaction.gatewayPayload)
            .MaximumLength(5000).When(transaction => !string.IsNullOrWhiteSpace(transaction.gatewayPayload))
            .WithMessage("{PropertyName} não pode ter mais de 5000 caracteres.");
    }
}