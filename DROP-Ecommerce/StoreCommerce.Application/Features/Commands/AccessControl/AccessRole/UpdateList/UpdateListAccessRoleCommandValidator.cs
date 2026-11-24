using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListAccessRoleCommandValidator : AbstractValidator<UpdateListAccessRoleCommand>
{
    public UpdateListAccessRoleCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new UpdateAccessRoleCommandValidator());
    }
}
