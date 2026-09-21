using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListOrderCommandValidator : AbstractValidator<DeleteListOrderCommand>
{
    public DeleteListOrderCommandValidator()
    {
        RuleFor(order => order.ids).NotEmpty();
        RuleForEach(order => order.ids).GreaterThan(0);
    }
}
