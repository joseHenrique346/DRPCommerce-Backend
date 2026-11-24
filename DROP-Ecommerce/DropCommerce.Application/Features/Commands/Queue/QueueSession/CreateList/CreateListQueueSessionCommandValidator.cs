using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListQueueSessionCommandValidator : AbstractValidator<CreateListQueueSessionCommand>
{
    public CreateListQueueSessionCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateQueueSessionCommandValidator());
    }
}
