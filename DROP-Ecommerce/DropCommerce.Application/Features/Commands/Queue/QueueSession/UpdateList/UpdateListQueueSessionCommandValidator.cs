using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListQueueSessionCommandValidator : AbstractValidator<UpdateListQueueSessionCommand>
{
    public UpdateListQueueSessionCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateQueueSessionCommandValidator());
    }
}
