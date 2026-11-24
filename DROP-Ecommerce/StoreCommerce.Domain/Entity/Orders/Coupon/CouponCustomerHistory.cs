namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Histórico do cliente usado nas condições do cupom: compras concluídas (confirmadas até entregues),
/// total gasto nelas e se ele já resgatou o cupom avaliado.
/// </summary>
public sealed record CouponCustomerHistory(int CompletedOrders, decimal TotalSpent, bool HasRedeemedCoupon);
