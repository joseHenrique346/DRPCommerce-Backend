using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class SetCouponActiveCommandValidator : AbstractValidator<SetCouponActiveCommand>
{
    public SetCouponActiveCommandValidator()
    {
        RuleFor(coupon => coupon.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
