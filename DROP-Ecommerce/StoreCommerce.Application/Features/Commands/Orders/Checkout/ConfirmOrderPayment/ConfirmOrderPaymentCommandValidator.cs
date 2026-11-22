using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ConfirmOrderPaymentCommandValidator : AbstractValidator<ConfirmOrderPaymentCommand>
{
    public ConfirmOrderPaymentCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.TransactionMethodId)
            .Must(methodId => BaseStaticEntity.GetAll<TransactionMethod>().Any(method => method.Id == methodId))
            .When(command => command.TransactionMethodId.HasValue).WithMessage("{PropertyName} inválido.");
        RuleFor(command => command.GatewayReference).MaximumLength(Transaction.GatewayReferenceMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(command => command.Note).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}
