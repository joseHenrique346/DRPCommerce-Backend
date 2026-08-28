using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Coupon;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateCouponCommand(long id, long enterpriseId, string code, long typeId, decimal discountValue, decimal minOrderValue, decimal maxDiscountCap, int? maxUses, int usedCount, bool isActive, bool isSingleUse, DateTime startsAt, DateTime expiresAt) : IRequest<Result<Coupon>> { }
