using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

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

        RuleFor(request => request.AddressId).GreaterThan(0).When(request => request.AddressId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(request => request.UseProfileAddress).Equal(false).When(request => request.AddressId.HasValue)
            .WithMessage("Informe addressId ou useProfileAddress, não os dois.");

        When(request => !request.UseProfileAddress && !request.AddressId.HasValue, () =>
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
