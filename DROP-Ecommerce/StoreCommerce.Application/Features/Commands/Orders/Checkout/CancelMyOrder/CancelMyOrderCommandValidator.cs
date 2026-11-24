using FluentValidation;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CancelMyOrderCommandValidator : AbstractValidator<CancelMyOrderCommand>
{
    public CancelMyOrderCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Reason).MaximumLength(OrderStatusHistory.ReasonMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}
