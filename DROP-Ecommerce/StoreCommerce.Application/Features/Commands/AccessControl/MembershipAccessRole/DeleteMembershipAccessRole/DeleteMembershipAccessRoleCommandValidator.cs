using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteMembershipAccessRoleCommandValidator : AbstractValidator<DeleteMembershipAccessRoleCommand>
{
    public DeleteMembershipAccessRoleCommandValidator()
    {
        RuleFor(command => command.enterpriseMembershipId).GreaterThan(0);
        RuleFor(command => command.accessRoleId).GreaterThan(0);
    }
}
