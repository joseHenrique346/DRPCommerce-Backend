using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateWaitlistEntryCommandValidator : AbstractValidator<CreateWaitlistEntryCommand>
{
    public CreateWaitlistEntryCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.dropProductId).GreaterThan(0).When(x => x.dropProductId.HasValue);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.position).GreaterThan(0);
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.joinedAt).NotEqual(default(DateTime));
        RuleFor(x => x.expiresAt).GreaterThan(x => x.joinedAt);
    }
}