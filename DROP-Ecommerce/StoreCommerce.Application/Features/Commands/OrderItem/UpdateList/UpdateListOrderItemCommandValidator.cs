using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListOrderItemCommandValidator : AbstractValidator<UpdateListOrderItemCommand>
{
    public UpdateListOrderItemCommandValidator()
    {
        RuleFor(orderItem => orderItem.commands).NotEmpty();
        RuleForEach(orderItem => orderItem.commands).SetValidator(new UpdateOrderItemCommandValidator());
    }
}
