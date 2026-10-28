using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateOrderItemCommandValidator : AbstractValidator<UpdateOrderItemCommand>
{
    public UpdateOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");

        RuleFor(orderItem => orderItem.quantity)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
