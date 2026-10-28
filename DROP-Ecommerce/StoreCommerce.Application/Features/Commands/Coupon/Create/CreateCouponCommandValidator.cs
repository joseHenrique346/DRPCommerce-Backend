using FluentValidation;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(coupon => coupon.code)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .Length(Coupon.CodeMinLength, Coupon.CodeMaxLength).WithMessage("{PropertyName} deve ter entre {MinLength} e {MaxLength} caracteres.");

        RuleFor(coupon => coupon.typeId)
            .Must(typeId => BaseStaticEntity.GetAll<CouponType>().Any(type => type.Id == typeId))
            .WithMessage("{PropertyName} inválido.");

        RuleFor(coupon => coupon.discountValue)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(coupon => coupon.discountValue)
            .LessThanOrEqualTo(Coupon.MaxPercentage).When(coupon => coupon.typeId == CouponType.Percentage.Id)
            .WithMessage("{PropertyName} não pode ser maior que 100% em cupons percentuais.");

        RuleFor(coupon => coupon.minOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(coupon => coupon.maxDiscountCap)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} não pode ser negativo.");

        RuleFor(coupon => coupon.maxUses)
            .GreaterThan(0).When(coupon => coupon.maxUses.HasValue)
            .WithMessage("{PropertyName} deve ser maior que zero quando fornecido.");

        RuleFor(coupon => coupon.startsAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.");

        RuleFor(coupon => coupon.expiresAt)
            .NotEqual(default(DateTime)).WithMessage("{PropertyName} deve ser uma data válida.")
            .GreaterThan(coupon => coupon.startsAt).WithMessage("{PropertyName} deve ser posterior a StartsAt.");
    }
}
