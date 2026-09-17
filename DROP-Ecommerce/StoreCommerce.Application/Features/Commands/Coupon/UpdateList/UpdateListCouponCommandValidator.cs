using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCouponCommandValidator : AbstractValidator<UpdateListCouponCommand>
{
    public UpdateListCouponCommandValidator()
    {
        RuleFor(coupon => coupon.commands).NotEmpty();
        RuleForEach(coupon => coupon.commands).SetValidator(new UpdateCouponCommandValidator());
    }
}
