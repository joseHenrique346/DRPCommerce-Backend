using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListOrderItemCommandValidator : AbstractValidator<DeleteListOrderItemCommand>
{
    public DeleteListOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.ids).NotEmpty();
        RuleForEach(orderItem => orderItem.ids).GreaterThan(0);
    }
}
