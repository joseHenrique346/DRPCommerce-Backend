using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteShipmentCommandValidator : AbstractValidator<DeleteShipmentCommand>
{
    public DeleteShipmentCommandValidator()
    {
        RuleFor(shipment => shipment.id)
            .GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
    }
}