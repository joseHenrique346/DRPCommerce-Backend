using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

public sealed record CartCouponPreview(string Code, bool IsValid, decimal DiscountAmount, string? ReasonCode, string? Message);
