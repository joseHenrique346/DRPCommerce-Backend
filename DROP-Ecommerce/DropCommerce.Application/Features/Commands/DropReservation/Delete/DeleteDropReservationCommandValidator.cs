using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropReservationCommandValidator : AbstractValidator<DeleteDropReservationCommand>
{
    public DeleteDropReservationCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}