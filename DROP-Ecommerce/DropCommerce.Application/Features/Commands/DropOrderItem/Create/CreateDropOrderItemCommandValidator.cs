using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropOrderItemCommandValidator : AbstractValidator<CreateDropOrderItemCommand>
{
    public CreateDropOrderItemCommandValidator()
    {
        RuleFor(x => x.dropOrderId).GreaterThan(0);
        RuleFor(x => x.dropProductId).GreaterThan(0);
        RuleFor(x => x.itemName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.quantity).GreaterThan(0);
        RuleFor(x => x.unitPrice).GreaterThan(0);
        RuleFor(x => x.totalPrice).GreaterThan(0);
    }
}