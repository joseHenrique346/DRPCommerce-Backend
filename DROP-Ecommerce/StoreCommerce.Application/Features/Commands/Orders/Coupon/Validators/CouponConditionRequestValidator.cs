using FluentValidation;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CouponConditionRequestValidator : AbstractValidator<CouponConditionRequest>
{
    public CouponConditionRequestValidator()
    {
        RuleFor(condition => condition.TypeId)
            .Must(typeId => BaseStaticEntity.GetAll<CouponConditionType>().Any(type => type.Id == typeId))
            .WithMessage("{PropertyName} inválido.");

        RuleFor(condition => condition.OperatorId)
            .Must(operatorId => BaseStaticEntity.GetAll<CouponConditionOperator>().Any(item => item.Id == operatorId))
            .WithMessage("{PropertyName} inválido.");

        RuleFor(condition => condition.Value)
            .InclusiveBetween(0, CouponCondition.MaxValue).WithMessage("{PropertyName} deve estar entre {From} e {To}.")
            .PrecisionScale(18, 2, true).WithMessage("{PropertyName} aceita no máximo 2 casas decimais.");

        RuleFor(condition => condition.Value)
            .GreaterThan(0).When(condition => condition.OperatorId == CouponConditionOperator.LessThan.Id)
            .WithMessage("A condição \"menor que 0\" nunca seria atendida.");

        RuleFor(condition => condition.Value)
            .Must(value => decimal.Truncate(value) == value).When(condition => !CouponConditionType.IsMonetary(condition.TypeId))
            .WithMessage("Condições de quantidade aceitam apenas números inteiros.");

        RuleFor(condition => condition.SecondValue)
            .NotNull().When(condition => CouponConditionOperator.RequiresSecondValue(condition.OperatorId))
            .WithMessage("{PropertyName} é obrigatório nos operadores de intervalo.");

        RuleFor(condition => condition.SecondValue)
            .Null().When(condition => !CouponConditionOperator.RequiresSecondValue(condition.OperatorId))
            .WithMessage("{PropertyName} só é usado pelos operadores de intervalo.");

        RuleFor(condition => condition.SecondValue.GetValueOrDefault())
            .GreaterThan(condition => condition.Value).WithName("SecondValue").WithMessage("{PropertyName} deve ser maior que Value.")
            .LessThanOrEqualTo(CouponCondition.MaxValue).WithName("SecondValue").WithMessage("{PropertyName} não pode ser maior que {ComparisonValue}.")
            .PrecisionScale(18, 2, true).WithName("SecondValue").WithMessage("{PropertyName} aceita no máximo 2 casas decimais.")
            .Must(value => decimal.Truncate(value) == value).When(condition => !CouponConditionType.IsMonetary(condition.TypeId), ApplyConditionTo.CurrentValidator)
            .WithName("SecondValue").WithMessage("Condições de quantidade aceitam apenas números inteiros.")
            .When(condition => condition.SecondValue.HasValue && CouponConditionOperator.RequiresSecondValue(condition.OperatorId));
    }
}
