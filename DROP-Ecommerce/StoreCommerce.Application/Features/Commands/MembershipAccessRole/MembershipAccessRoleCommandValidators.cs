using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateMembershipAccessRoleCommandValidator : AbstractValidator<CreateMembershipAccessRoleCommand>
{
    public CreateMembershipAccessRoleCommandValidator()
    {
        RuleFor(command => command.enterpriseId).GreaterThan(0);
        RuleFor(command => command.enterpriseMembershipId).GreaterThan(0);
        RuleFor(command => command.accessRoleId).GreaterThan(0);
    }
}

public class DeleteMembershipAccessRoleCommandValidator : AbstractValidator<DeleteMembershipAccessRoleCommand>
{
    public DeleteMembershipAccessRoleCommandValidator()
    {
        RuleFor(command => command.enterpriseMembershipId).GreaterThan(0);
        RuleFor(command => command.accessRoleId).GreaterThan(0);
    }
}
