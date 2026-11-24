using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropReservationCommandValidator : AbstractValidator<UpdateListDropReservationCommand>
{
    public UpdateListDropReservationCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropReservationCommandValidator());
    }
}
