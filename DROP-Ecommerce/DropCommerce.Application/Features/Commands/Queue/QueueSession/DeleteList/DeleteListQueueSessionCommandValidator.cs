using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueSessionCommandValidator : AbstractValidator<DeleteListQueueSessionCommand>
{
    public DeleteListQueueSessionCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
