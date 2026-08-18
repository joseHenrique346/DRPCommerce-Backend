using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropNotificationCommandValidator : AbstractValidator<CreateDropNotificationCommand>
{
    public CreateDropNotificationCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.channelId).GreaterThan(0);
        RuleFor(x => x.typeId).GreaterThan(0);
        RuleFor(x => x.subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.body).NotEmpty();
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.scheduledAt).NotEqual(default(DateTime));
    }
}