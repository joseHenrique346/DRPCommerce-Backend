using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(r => r.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
