using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteWaitlistEntryCommandValidator : AbstractValidator<DeleteWaitlistEntryCommand>
{
    public DeleteWaitlistEntryCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}