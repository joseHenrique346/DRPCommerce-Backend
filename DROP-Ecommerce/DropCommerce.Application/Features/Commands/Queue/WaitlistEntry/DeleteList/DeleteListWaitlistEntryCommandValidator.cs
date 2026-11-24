using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListWaitlistEntryCommandValidator : AbstractValidator<DeleteListWaitlistEntryCommand>
{
    public DeleteListWaitlistEntryCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
