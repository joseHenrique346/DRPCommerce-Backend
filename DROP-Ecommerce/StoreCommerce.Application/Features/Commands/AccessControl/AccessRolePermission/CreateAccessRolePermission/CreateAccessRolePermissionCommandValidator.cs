using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateAccessRolePermissionCommandValidator : AbstractValidator<CreateAccessRolePermissionCommand>
{
    public CreateAccessRolePermissionCommandValidator()
    {
        RuleFor(command => command.accessRoleId).GreaterThan(0);
        RuleFor(command => command.permissionId).GreaterThan(0);
    }
}
