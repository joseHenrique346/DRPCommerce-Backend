using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropCouponCommandValidator : AbstractValidator<CreateListDropCouponCommand>
{
    public CreateListDropCouponCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropCouponCommandValidator());
    }
}
