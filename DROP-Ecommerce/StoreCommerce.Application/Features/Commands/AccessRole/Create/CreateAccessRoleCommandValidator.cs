using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateAccessRoleCommandValidator : AbstractValidator<CreateAccessRoleCommand>
{
    public CreateAccessRoleCommandValidator()
    {
        RuleFor(command => command.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(AccessRole.NameMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");

        RuleFor(command => command.description)
            .MaximumLength(AccessRole.DescriptionMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}
