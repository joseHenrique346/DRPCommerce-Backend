using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListUserCommandValidator : AbstractValidator<DeleteListUserCommand>
{
    public DeleteListUserCommandValidator()
    {
        RuleFor(command => command.ids).NotEmpty();
        RuleForEach(command => command.ids).GreaterThan(0);
    }
}
