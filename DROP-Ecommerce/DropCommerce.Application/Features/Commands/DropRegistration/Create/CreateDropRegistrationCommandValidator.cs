using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropRegistrationCommandValidator : AbstractValidator<CreateDropRegistrationCommand>
{
    public CreateDropRegistrationCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.eligibilityReason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.registeredAt).NotEqual(default(DateTime));
    }
}