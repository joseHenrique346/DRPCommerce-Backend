using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(coupon => coupon.enterpriseId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(coupon => coupon.code)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(50).WithMessage("{PropertyName} não pode ter mais de 50 caracteres.");

        RuleFor(coupon => coupon.typeId)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(coupon => coupon.discountValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(coupon => coupon.minOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(coupon => coupon.maxDiscountCap)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(coupon => coupon.maxUses)
            .GreaterThan(0).When(coupon => coupon.maxUses.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(coupon => coupon.usedCount)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(coupon => coupon.startsAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(coupon => coupon.expiresAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.")
            .GreaterThan(coupon => coupon.startsAt).WithMessage("{PropertyName} deve ser posterior a StartsAt.");
    }
}