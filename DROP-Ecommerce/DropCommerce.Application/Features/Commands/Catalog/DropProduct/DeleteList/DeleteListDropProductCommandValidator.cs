using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropProductCommandValidator : AbstractValidator<DeleteListDropProductCommand>
{
    public DeleteListDropProductCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
