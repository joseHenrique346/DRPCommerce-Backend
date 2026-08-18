using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropCouponCommandValidator : AbstractValidator<DeleteDropCouponCommand>
{
    public DeleteDropCouponCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}