using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListQueueEntryCommandValidator : AbstractValidator<UpdateListQueueEntryCommand>
{
    public UpdateListQueueEntryCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateQueueEntryCommandValidator());
    }
}
