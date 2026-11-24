using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteAccessRolePermissionCommandValidator : AbstractValidator<DeleteAccessRolePermissionCommand>
{
    public DeleteAccessRolePermissionCommandValidator()
    {
        RuleFor(command => command.accessRoleId).GreaterThan(0);
        RuleFor(command => command.permissionId).GreaterThan(0);
    }
}
