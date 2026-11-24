using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Contracts.Coupon;

/// <summary>
/// Resultado da validação de um código. Quando inválido, ReasonCode e Message explicam o motivo.
/// </summary>
public sealed record CouponValidationResponse(string Code, bool IsValid, decimal DiscountAmount, decimal SubTotalAfterDiscount, string? ReasonCode, string? Message, IReadOnlyList<CouponConditionResponse> Conditions);
