namespace StoreCommerce.Domain.StaticEntity;

/// <summary>
/// Comparação aplicada pela condição do cupom. "Entre" inclui os limites; "Maior que e menor que" os exclui.
/// </summary>
public sealed class CouponConditionOperator : BaseStaticEntity
{
    public static readonly CouponConditionOperator Equal = new(1, "Igual a");
    public static readonly CouponConditionOperator GreaterThan = new(2, "Maior que");
    public static readonly CouponConditionOperator GreaterThanOrEqual = new(3, "Maior ou igual a");
    public static readonly CouponConditionOperator LessThan = new(4, "Menor que");
    public static readonly CouponConditionOperator LessThanOrEqual = new(5, "Menor ou igual a");
    public static readonly CouponConditionOperator Between = new(6, "Entre (inclusive)");
    public static readonly CouponConditionOperator GreaterThanAndLessThan = new(7, "Maior que e menor que");

    private CouponConditionOperator(long id, string description) : base(id, description) { }

    public static bool RequiresSecondValue(long id) => id == Between.Id || id == GreaterThanAndLessThan.Id;

    public static bool Evaluate(long id, decimal actual, decimal value, decimal? secondValue)
    {
        if (id == Equal.Id) return actual == value;
        if (id == GreaterThan.Id) return actual > value;
        if (id == GreaterThanOrEqual.Id) return actual >= value;
        if (id == LessThan.Id) return actual < value;
        if (id == LessThanOrEqual.Id) return actual <= value;
        if (id == Between.Id) return actual >= value && actual <= secondValue!.Value;
        if (id == GreaterThanAndLessThan.Id) return actual > value && actual < secondValue!.Value;

        throw new ArgumentException("Operador de condição inválido.");
    }
}
