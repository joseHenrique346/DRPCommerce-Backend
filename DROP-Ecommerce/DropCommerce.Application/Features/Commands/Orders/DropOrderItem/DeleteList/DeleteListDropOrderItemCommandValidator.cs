using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderItemCommandValidator : AbstractValidator<DeleteListDropOrderItemCommand>
{
    public DeleteListDropOrderItemCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
