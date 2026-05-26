using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropOrderItemCommandValidator : AbstractValidator<DeleteDropOrderItemCommand>
{
    public DeleteDropOrderItemCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}