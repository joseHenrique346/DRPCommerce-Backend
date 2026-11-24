using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeliverOrderCommandValidator
    : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}
