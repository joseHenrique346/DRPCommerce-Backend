using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateAccessRoleCommandValidator : AbstractValidator<CreateAccessRoleCommand>
{
    public CreateAccessRoleCommandValidator()
    {
        RuleFor(command => command.enterpriseId).GreaterThan(0);
        RuleFor(command => command.name).NotEmpty().MaximumLength(255);
        RuleFor(command => command.description).MaximumLength(1000);
    }
}
