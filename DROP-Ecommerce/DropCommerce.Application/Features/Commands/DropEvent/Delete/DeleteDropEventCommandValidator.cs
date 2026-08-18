using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropEventCommandValidator : AbstractValidator<DeleteDropEventCommand>
{
    public DeleteDropEventCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}