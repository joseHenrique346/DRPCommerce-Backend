using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListAccessRoleCommandValidator : AbstractValidator<DeleteListAccessRoleCommand>
{
    public DeleteListAccessRoleCommandValidator()
    {
        RuleFor(command => command.ids).NotEmpty();
        RuleForEach(command => command.ids).GreaterThan(0);
    }
}
