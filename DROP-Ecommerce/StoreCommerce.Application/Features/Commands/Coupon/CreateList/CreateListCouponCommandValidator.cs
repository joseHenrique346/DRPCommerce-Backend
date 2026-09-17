using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCouponCommandValidator : AbstractValidator<CreateListCouponCommand>
{
    public CreateListCouponCommandValidator()
    {
        RuleFor(coupon => coupon.commands).NotEmpty();
        RuleForEach(coupon => coupon.commands).SetValidator(new CreateCouponCommandValidator());
    }
}
