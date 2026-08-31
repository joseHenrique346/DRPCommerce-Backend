using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropNotificationCommandValidator : AbstractValidator<DeleteListDropNotificationCommand>
{
    public DeleteListDropNotificationCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
