using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ApplyPaymentResultCommandValidator : AbstractValidator<ApplyPaymentResultCommand>
{
    public ApplyPaymentResultCommandValidator()
    {
        RuleFor(command => command.Provider).NotEmpty().MaximumLength(Transaction.GatewayProviderMaxLength);
        RuleFor(command => command.Reference).NotEmpty().MaximumLength(Transaction.GatewayReferenceMaxLength);
        RuleFor(command => command.Status).IsInEnum();
        RuleFor(command => command.PaidAmount).GreaterThanOrEqualTo(0).When(command => command.PaidAmount.HasValue);
    }
}
