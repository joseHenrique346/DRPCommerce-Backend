using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueEntryCommandValidator : AbstractValidator<DeleteListQueueEntryCommand>
{
    public DeleteListQueueEntryCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
