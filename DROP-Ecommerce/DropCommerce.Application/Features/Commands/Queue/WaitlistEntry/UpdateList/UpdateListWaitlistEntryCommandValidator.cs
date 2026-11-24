using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListWaitlistEntryCommandValidator : AbstractValidator<UpdateListWaitlistEntryCommand>
{
    public UpdateListWaitlistEntryCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateWaitlistEntryCommandValidator());
    }
}
