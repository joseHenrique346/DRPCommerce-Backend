using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropProductCommandValidator : AbstractValidator<UpdateDropProductCommand>
{
    public UpdateDropProductCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.productId).GreaterThan(0);
        RuleFor(x => x.sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.unitsAllocated).GreaterThan(0);
        RuleFor(x => x.unitsSold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.maxPerCustomer).GreaterThan(0);
        RuleFor(x => x.price).GreaterThan(0);
    }
}