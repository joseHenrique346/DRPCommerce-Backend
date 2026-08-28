using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropEventCommandValidator : AbstractValidator<DeleteListDropEventCommand>
{
    public DeleteListDropEventCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
