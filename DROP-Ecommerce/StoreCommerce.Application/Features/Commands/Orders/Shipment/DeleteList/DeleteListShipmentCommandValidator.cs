using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListShipmentCommandValidator : AbstractValidator<DeleteListShipmentCommand>
{
    public DeleteListShipmentCommandValidator()
    {
        RuleFor(shipment => shipment.ids).NotEmpty();
        RuleForEach(shipment => shipment.ids).GreaterThan(0);
    }
}
