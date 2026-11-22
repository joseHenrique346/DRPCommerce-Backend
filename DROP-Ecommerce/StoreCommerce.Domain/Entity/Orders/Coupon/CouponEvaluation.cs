namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Resultado da avaliação do cupom: desconto calculado ou o motivo da recusa.
/// </summary>
public sealed record CouponEvaluation(bool IsValid, decimal DiscountAmount, string? ErrorCode, string? Message)
{
    public static CouponEvaluation Valid(decimal discountAmount) => new(true, discountAmount, null, null);

    public static CouponEvaluation Invalid(string errorCode, string message) => new(false, 0, errorCode, message);
}
