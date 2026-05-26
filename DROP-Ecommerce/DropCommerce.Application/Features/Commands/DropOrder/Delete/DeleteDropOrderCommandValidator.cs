using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropOrderCommandValidator : AbstractValidator<DeleteDropOrderCommand>
{
    public DeleteDropOrderCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}