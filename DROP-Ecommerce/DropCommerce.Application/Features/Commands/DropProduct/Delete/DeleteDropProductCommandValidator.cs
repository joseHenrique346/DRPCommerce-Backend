using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropProductCommandValidator : AbstractValidator<DeleteDropProductCommand>
{
    public DeleteDropProductCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}