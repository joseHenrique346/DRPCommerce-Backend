using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.id).GreaterThan(0);
        RuleFor(command => command.fullName).NotEmpty().MaximumLength(255);
        RuleFor(command => command.email).NotEmpty().EmailAddress().MaximumLength(255);
    }
}
