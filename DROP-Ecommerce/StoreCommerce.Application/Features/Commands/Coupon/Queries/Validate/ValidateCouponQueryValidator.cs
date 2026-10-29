using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ValidateCouponQueryValidator : AbstractValidator<ValidateCouponQuery>
{
    public ValidateCouponQueryValidator()
    {
        RuleFor(query => query.Code)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(Coupon.CodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(query => query.SubTotal)
            .InclusiveBetween(0, CouponCondition.MaxValue).WithMessage("{PropertyName} deve estar entre {From} e {To}.");

        RuleFor(query => query.ItemQuantity)
            .InclusiveBetween(0, StoreSettings.MaxItemsPerOrderLimit).WithMessage("{PropertyName} deve estar entre {From} e {To}.");

        RuleFor(query => query.CustomerId)
            .GreaterThan(0).When(query => query.CustomerId.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");
    }
}
