using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropNotificationCommandValidator : AbstractValidator<UpdateListDropNotificationCommand>
{
    public UpdateListDropNotificationCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropNotificationCommandValidator());
    }
}
