using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListWaitlistEntryCommandValidator : AbstractValidator<CreateListWaitlistEntryCommand>
{
    public CreateListWaitlistEntryCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateWaitlistEntryCommandValidator());
    }
}
