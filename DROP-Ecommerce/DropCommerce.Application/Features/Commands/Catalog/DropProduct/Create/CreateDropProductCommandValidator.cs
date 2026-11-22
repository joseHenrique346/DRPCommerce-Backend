using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropProductCommandValidator : AbstractValidator<CreateDropProductCommand>
{
    public CreateDropProductCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(Command => Command.productId).GreaterThan(0).When(Command => Command.productId.HasValue);
        RuleFor(x => x.sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.unitsAllocated).GreaterThan(0);
        RuleFor(x => x.unitsSold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.maxPerCustomer).GreaterThan(0);
        RuleFor(x => x.price).GreaterThan(0);
    }
}
