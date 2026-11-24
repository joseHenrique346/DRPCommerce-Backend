using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropNotificationCommandValidator : AbstractValidator<CreateListDropNotificationCommand>
{
    public CreateListDropNotificationCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropNotificationCommandValidator());
    }
}
