using System.Text.RegularExpressions;
using FluentValidation;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(coupon => coupon.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(coupon => coupon.code)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .Must(code => Coupon.NormalizeCode(code).Length is >= Coupon.CodeMinLength and <= Coupon.CodeMaxLength)
            .WithMessage($"{{PropertyName}} deve ter entre {Coupon.CodeMinLength} e {Coupon.CodeMaxLength} caracteres, sem contar espaços.")
            .Must(code => Regex.IsMatch(Coupon.NormalizeCode(code), Coupon.CodePattern))
            .WithMessage("{PropertyName} aceita apenas letras sem acento, números, hífen e sublinhado.");

        RuleFor(coupon => coupon.typeId)
            .Must(typeId => BaseStaticEntity.GetAll<CouponType>().Any(type => type.Id == typeId))
            .WithMessage("{PropertyName} inválido.");

        RuleFor(coupon => coupon.discountValue)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(coupon => coupon.discountValue)
            .LessThanOrEqualTo(Coupon.MaxPercentage).When(coupon => coupon.typeId == CouponType.Percentage.Id)
            .WithMessage("{PropertyName} não pode ser maior que 100% em cupons percentuais.");

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

        RuleFor(coupon => coupon.conditions)
            .Must(conditions => conditions is null || conditions.Count <= Coupon.MaxConditions)
            .WithMessage($"Informe no máximo {Coupon.MaxConditions} condições.")
            .Must(conditions => conditions is null || conditions.Select(condition => condition.TypeId).Distinct().Count() == conditions.Count)
            .WithMessage("Use no máximo uma condição de cada tipo.");

        RuleForEach(coupon => coupon.conditions).SetValidator(new CouponConditionRequestValidator());
    }
}
