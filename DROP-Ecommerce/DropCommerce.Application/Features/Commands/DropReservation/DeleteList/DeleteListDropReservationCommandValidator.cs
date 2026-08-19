using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropReservationCommandValidator : AbstractValidator<DeleteListDropReservationCommand>
{
    public DeleteListDropReservationCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
