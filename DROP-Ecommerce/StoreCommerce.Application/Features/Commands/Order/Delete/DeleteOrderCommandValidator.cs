using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(o => o.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
