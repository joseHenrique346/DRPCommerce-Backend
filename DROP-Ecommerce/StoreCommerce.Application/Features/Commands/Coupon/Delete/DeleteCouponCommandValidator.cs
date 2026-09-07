using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteCouponCommandValidator : AbstractValidator<DeleteCouponCommand>
{
    public DeleteCouponCommandValidator()
    {
        RuleFor(c => c.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
