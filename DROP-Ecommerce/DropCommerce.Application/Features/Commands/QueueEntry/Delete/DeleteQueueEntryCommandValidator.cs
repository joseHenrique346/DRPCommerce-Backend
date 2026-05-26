using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteQueueEntryCommandValidator : AbstractValidator<DeleteQueueEntryCommand>
{
    public DeleteQueueEntryCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}