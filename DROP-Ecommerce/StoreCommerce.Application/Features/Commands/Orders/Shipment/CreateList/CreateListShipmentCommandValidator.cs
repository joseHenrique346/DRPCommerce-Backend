using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListShipmentCommandValidator : AbstractValidator<CreateListShipmentCommand>
{
    public CreateListShipmentCommandValidator()
    {
        RuleFor(shipment => shipment.commands).NotEmpty();
        RuleForEach(shipment => shipment.commands).SetValidator(new CreateShipmentCommandValidator());
    }
}
