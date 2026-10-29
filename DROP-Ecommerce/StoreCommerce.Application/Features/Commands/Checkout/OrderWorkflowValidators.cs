using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.Request).NotNull().SetValidator(new CheckoutRequestValidator());
    }
}

public sealed class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(request => request.IdempotencyKey)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Order.IdempotencyKeyMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("{PropertyName} aceita apenas letras, números, hífen e sublinhado.");

        RuleFor(request => request.PaymentMethodId)
            .Must(methodId => BaseStaticEntity.GetAll<TransactionMethod>().Any(method => method.Id == methodId))
            .WithMessage("{PropertyName} inválido.");

        RuleFor(request => request.CouponCode)
            .MaximumLength(Coupon.CodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(request => request.Notes)
            .MaximumLength(Order.NotesMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        When(request => !request.UseProfileAddress, () =>
        {
            RuleFor(request => request.ShippingAddressLine)
                .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
                .MaximumLength(Order.ShippingAddressLineMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

            RuleFor(request => request.ShippingCity)
                .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
                .MaximumLength(Order.ShippingCityMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

            RuleFor(request => request.ShippingStateId)
                .NotNull().WithMessage("{PropertyName} não pode ser vazio.")
                .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

            RuleFor(request => request.ShippingZipCode)
                .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
                .MaximumLength(Order.ShippingZipCodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        });
    }
}

public sealed class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    public GetMyOrdersQueryValidator()
    {
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class CancelMyOrderCommandValidator : AbstractValidator<CancelMyOrderCommand>
{
    public CancelMyOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Reason).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

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

public sealed class AdvanceOrderCommandValidator : AbstractValidator<AdvanceOrderCommand>
{
    public AdvanceOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Step).IsInEnum();
        RuleFor(command => command.Note).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Reason)
            .NotEmpty().WithMessage("Informe o motivo do cancelamento.")
            .MaximumLength(OrderStatusHistory.ReasonMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

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
