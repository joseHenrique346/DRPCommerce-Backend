using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteEnterpriseCommandValidator : AbstractValidator<DeleteEnterpriseCommand>
{
    public DeleteEnterpriseCommandValidator()
    {
        RuleFor(e => e.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}