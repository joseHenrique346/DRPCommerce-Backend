using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropReservationCommandValidator : AbstractValidator<CreateDropReservationCommand>
{
    public CreateDropReservationCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.dropProductId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.queueEntryId).GreaterThan(0);
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.quantity).GreaterThan(0);
        RuleFor(x => x.unitPrice).GreaterThan(0);
        RuleFor(x => x.totalAmount).GreaterThan(0);
        RuleFor(x => x.lockToken).NotEmpty();
        RuleFor(x => x.reservedAt).NotEqual(default(DateTime));
        RuleFor(x => x.expiresAt).GreaterThan(x => x.reservedAt);
    }
}