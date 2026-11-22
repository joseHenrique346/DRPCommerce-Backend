using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.fullName).NotEmpty().MaximumLength(255);
        RuleFor(command => command.email).NotEmpty().EmailAddress().MaximumLength(255);
    }
}
