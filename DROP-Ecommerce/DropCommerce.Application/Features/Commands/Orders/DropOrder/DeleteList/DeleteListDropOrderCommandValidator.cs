using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderCommandValidator : AbstractValidator<DeleteListDropOrderCommand>
{
    public DeleteListDropOrderCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
