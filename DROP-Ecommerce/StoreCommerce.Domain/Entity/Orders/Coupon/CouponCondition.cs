using System.Globalization;
using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Regra de elegibilidade do cupom (ex.: valor do pedido maior que 1000; compras anteriores do cliente igual a 0).
/// Todas as condições de um cupom precisam ser atendidas.
/// </summary>
public class CouponCondition
{
    #region Constants
    public const decimal MaxValue = 9_999_999_999_999_999.99m;
    #endregion

    #region Properties
    public long CouponConditionTypeId { get; private set; }
    public long CouponConditionOperatorId { get; private set; }
    public decimal Value { get; private set; }
    public decimal? SecondValue { get; private set; }
    #endregion

    #region Constructor
    protected CouponCondition() { }
    #endregion

    #region Functions
    public static CouponCondition Create(long conditionTypeId, long operatorId, decimal value, decimal? secondValue)
    {
        if (!BaseStaticEntity.GetAll<CouponConditionType>().Any(type => type.Id == conditionTypeId))
            throw new ArgumentException($"{nameof(CouponConditionTypeId)} is invalid.");

        if (!BaseStaticEntity.GetAll<CouponConditionOperator>().Any(item => item.Id == operatorId))
            throw new ArgumentException($"{nameof(CouponConditionOperatorId)} is invalid.");

        BaseValidate.ValidateRange(value, 0, MaxValue, nameof(Value));

        if (CouponConditionOperator.RequiresSecondValue(operatorId))
        {
            if (!secondValue.HasValue)
                throw new ArgumentException($"{nameof(SecondValue)} é obrigatório para este operador.");

            BaseValidate.ValidateRange(secondValue.Value, 0, MaxValue, nameof(SecondValue));
            BaseValidate.ValidateGreaterThan(secondValue.Value, value, nameof(SecondValue));
        }
        else if (secondValue.HasValue)
        {
            throw new ArgumentException($"{nameof(SecondValue)} só é usado pelos operadores de intervalo.");
        }

        if (operatorId == CouponConditionOperator.LessThan.Id && value == 0)
            throw new ArgumentException("A condição \"menor que 0\" nunca seria atendida.");

        if (!CouponConditionType.IsMonetary(conditionTypeId) &&
            (decimal.Truncate(value) != value || (secondValue.HasValue && decimal.Truncate(secondValue.Value) != secondValue.Value)))
            throw new ArgumentException("Condições de quantidade aceitam apenas números inteiros.");

        if (decimal.Round(value, 2) != value || (secondValue.HasValue && decimal.Round(secondValue.Value, 2) != secondValue.Value))
            throw new ArgumentException("Os valores da condição aceitam no máximo 2 casas decimais.");

        return new CouponCondition
        {
            CouponConditionTypeId = conditionTypeId,
            CouponConditionOperatorId = operatorId,
            Value = value,
            SecondValue = secondValue
        };
    }

    public bool IsCustomerScoped => CouponConditionType.IsCustomerScoped(CouponConditionTypeId);

    public bool IsSatisfiedBy(decimal actual) =>
        CouponConditionOperator.Evaluate(CouponConditionOperatorId, actual, Value, SecondValue);

    public bool IsEquivalentTo(CouponCondition other) =>
        CouponConditionTypeId == other.CouponConditionTypeId && CouponConditionOperatorId == other.CouponConditionOperatorId &&
        Value == other.Value && SecondValue == other.SecondValue;

    /// <summary>
    /// Texto legível da condição, ex.: "valor do pedido maior que 1.000,00".
    /// </summary>
    public string Describe()
    {
        var type = BaseStaticEntity.GetAll<CouponConditionType>().First(item => item.Id == CouponConditionTypeId);
        var first = Format(Value);
        var phrase = CouponConditionOperatorId switch
        {
            1 => $"igual a {first}",
            2 => $"maior que {first}",
            3 => $"maior ou igual a {first}",
            4 => $"menor que {first}",
            5 => $"menor ou igual a {first}",
            6 => $"entre {first} e {Format(SecondValue!.Value)}",
            _ => $"maior que {first} e menor que {Format(SecondValue!.Value)}"
        };

        return $"{type.Description.ToLowerInvariant()} {phrase}";
    }

    private string Format(decimal value) =>
        value.ToString(CouponConditionType.IsMonetary(CouponConditionTypeId) ? "N2" : "N0", CultureInfo.GetCultureInfo("pt-BR"));
    #endregion
}
