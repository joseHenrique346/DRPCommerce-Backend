using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropCouponCommandValidator : AbstractValidator<CreateDropCouponCommand>
{
    public CreateDropCouponCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.typeId).GreaterThan(0);
        RuleFor(x => x.discountValue).GreaterThan(0);
        RuleFor(x => x.minOrderValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.maxDiscountCap).GreaterThanOrEqualTo(0);
        RuleFor(x => x.maxUses).GreaterThan(0);
        RuleFor(x => x.usedCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.usedCount).LessThanOrEqualTo(x => x.maxUses);
        RuleFor(x => x.startsAt).NotEqual(default(DateTime));
        RuleFor(x => x.expiresAt).GreaterThan(x => x.startsAt);
    }
}