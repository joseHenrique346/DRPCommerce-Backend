using MediatR;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Atualiza o cupom. Depois do primeiro uso, apenas expiresAt, maxUses e isActive podem mudar.
/// </summary>
public record class UpdateCouponCommand(long id, string code, long typeId, decimal discountValue, decimal maxDiscountCap, int? maxUses, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt, List<CouponConditionRequest>? conditions) : IRequest<Result<Coupon>> { }
