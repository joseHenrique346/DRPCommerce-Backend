using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(order => order.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}