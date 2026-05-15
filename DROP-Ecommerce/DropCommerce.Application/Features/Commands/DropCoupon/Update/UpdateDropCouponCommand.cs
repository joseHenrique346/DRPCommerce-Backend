using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class UpdateDropCouponCommand(long id, long dropEventId, string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int maxUses, int usedCount, bool isActive, bool isSingleUse, bool isExclusiveToRegistered, DateTime startsAt, DateTime expiresAt) : IRequest<Result<DropCoupon>> { }