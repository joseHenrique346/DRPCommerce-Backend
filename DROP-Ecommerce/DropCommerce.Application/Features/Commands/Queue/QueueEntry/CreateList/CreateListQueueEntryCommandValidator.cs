using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListQueueEntryCommandValidator : AbstractValidator<CreateListQueueEntryCommand>
{
    public CreateListQueueEntryCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateQueueEntryCommandValidator());
    }
}
