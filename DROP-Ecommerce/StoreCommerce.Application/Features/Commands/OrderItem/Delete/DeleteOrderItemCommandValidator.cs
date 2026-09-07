using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteOrderItemCommandValidator : AbstractValidator<DeleteOrderItemCommand>
{
    public DeleteOrderItemCommandValidator()
    {
        RuleFor(oi => oi.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}