using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListCouponCommandValidator : AbstractValidator<DeleteListCouponCommand>
{
    public DeleteListCouponCommandValidator()
    {
        RuleFor(coupon => coupon.ids).NotEmpty();
        RuleForEach(coupon => coupon.ids).GreaterThan(0);
    }
}
