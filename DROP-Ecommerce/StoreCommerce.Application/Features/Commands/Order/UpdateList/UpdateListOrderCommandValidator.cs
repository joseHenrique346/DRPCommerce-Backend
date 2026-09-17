using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListOrderCommandValidator : AbstractValidator<UpdateListOrderCommand>
{
    public UpdateListOrderCommandValidator()
    {
        RuleFor(order => order.commands).NotEmpty();
        RuleForEach(order => order.commands).SetValidator(new UpdateOrderCommandValidator());
    }
}
