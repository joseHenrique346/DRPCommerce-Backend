using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListAccessRoleCommandValidator : AbstractValidator<CreateListAccessRoleCommand>
{
    public CreateListAccessRoleCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new CreateAccessRoleCommandValidator());
    }
}
