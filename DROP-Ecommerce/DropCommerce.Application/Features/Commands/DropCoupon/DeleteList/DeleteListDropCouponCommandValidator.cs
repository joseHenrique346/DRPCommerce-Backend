using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropCouponCommandValidator : AbstractValidator<DeleteListDropCouponCommand>
{
    public DeleteListDropCouponCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
