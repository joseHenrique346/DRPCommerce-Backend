using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(r => r.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(r => r.name)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(255).WithMessage("{PropertyName} não pode ter mais de 255 caracteres.");

        RuleFor(r => r.description)
            .MaximumLength(1000).WithMessage("{PropertyName} não pode ter mais de 1000 caracteres.");
    }
}
