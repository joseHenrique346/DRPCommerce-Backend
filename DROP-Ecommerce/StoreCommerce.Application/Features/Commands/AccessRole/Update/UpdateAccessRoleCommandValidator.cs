using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateAccessRoleCommandValidator : AbstractValidator<UpdateAccessRoleCommand>
{
    public UpdateAccessRoleCommandValidator()
    {
        RuleFor(command => command.id).GreaterThan(0);
        RuleFor(command => command.name).NotEmpty().MaximumLength(255);
        RuleFor(command => command.description).MaximumLength(1000);
    }
}
