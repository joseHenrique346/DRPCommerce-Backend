using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListOrderCommandValidator : AbstractValidator<CreateListOrderCommand>
{
    public CreateListOrderCommandValidator()
    {
        RuleFor(order => order.commands).NotEmpty();
        RuleForEach(order => order.commands).SetValidator(new CreateOrderCommandValidator());
    }
}
