namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Histórico do cliente usado nas condições do cupom: compras concluídas (confirmadas até entregues),
/// total gasto nelas e se ele já resgatou o cupom avaliado.
/// </summary>
public sealed record CouponCustomerHistory(int CompletedOrders, decimal TotalSpent, bool HasRedeemedCoupon);

/// <summary>
/// Resultado da avaliação do cupom: desconto calculado ou o motivo da recusa.
/// </summary>
public sealed record CouponEvaluation(bool IsValid, decimal DiscountAmount, string? ErrorCode, string? Message)
{
    public static CouponEvaluation Valid(decimal discountAmount) => new(true, discountAmount, null, null);

    public static CouponEvaluation Invalid(string errorCode, string message) => new(false, 0, errorCode, message);
}

/// <summary>
/// Situação do cupom no momento da consulta.
/// </summary>
public enum CouponAvailability
{
    Scheduled = 1,
    Active = 2,
    Expired = 3,
    Exhausted = 4,
    Inactive = 5
}
