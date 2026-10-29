using MediatR;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Cria um cupom. O código é gravado sem espaços e em maiúsculas; conditions define quem e quando pode usar (todas precisam ser atendidas).
/// </summary>
public record class CreateCouponCommand(string code, long typeId, decimal discountValue, decimal maxDiscountCap, int? maxUses, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt, List<CouponConditionRequest>? conditions) : IRequest<Result<Coupon>> { }
