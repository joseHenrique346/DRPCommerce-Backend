using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListUserCommandValidator : AbstractValidator<CreateListUserCommand>
{
    public CreateListUserCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new CreateUserCommandValidator());
    }
}
