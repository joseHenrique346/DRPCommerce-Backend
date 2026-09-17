using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListShipmentCommandValidator : AbstractValidator<UpdateListShipmentCommand>
{
    public UpdateListShipmentCommandValidator()
    {
        RuleFor(shipment => shipment.commands).NotEmpty();
        RuleForEach(shipment => shipment.commands).SetValidator(new UpdateShipmentCommandValidator());
    }
}
