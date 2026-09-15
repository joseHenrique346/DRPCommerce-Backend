using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteOrderItemCommandValidator : AbstractValidator<DeleteOrderItemCommand>
{
    public DeleteOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}