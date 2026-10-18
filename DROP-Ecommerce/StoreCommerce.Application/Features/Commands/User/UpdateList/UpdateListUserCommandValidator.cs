using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListUserCommandValidator : AbstractValidator<UpdateListUserCommand>
{
    public UpdateListUserCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new UpdateUserCommandValidator());
    }
}
