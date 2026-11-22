using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Contracts.Coupon;

/// <summary>
/// Condição do cupom. Exemplos: valor do pedido (1) maior que (2) 1000; compras anteriores do cliente (3) igual a (1) 0.
/// SecondValue é usado apenas pelos operadores de intervalo (6 = entre, 7 = maior que e menor que).
/// </summary>
public sealed record CouponConditionRequest(long TypeId, long OperatorId, decimal Value, decimal? SecondValue);
