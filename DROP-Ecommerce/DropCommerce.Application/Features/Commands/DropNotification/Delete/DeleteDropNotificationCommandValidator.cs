using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropNotificationCommandValidator : AbstractValidator<DeleteDropNotificationCommand>
{
    public DeleteDropNotificationCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}