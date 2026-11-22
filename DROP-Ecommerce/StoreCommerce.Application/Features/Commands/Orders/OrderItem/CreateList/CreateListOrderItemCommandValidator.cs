using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListOrderItemCommandValidator : AbstractValidator<CreateListOrderItemCommand>
{
    public CreateListOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.commands).NotEmpty();
        RuleForEach(orderItem => orderItem.commands).SetValidator(new CreateOrderItemCommandValidator());
    }
}
