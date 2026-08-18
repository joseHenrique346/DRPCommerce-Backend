using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateQueueSessionCommandValidator : AbstractValidator<UpdateQueueSessionCommand>
{
    public UpdateQueueSessionCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
        RuleFor(x => x.queueEntryId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.token).NotEmpty();
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.issuedAt).NotEqual(default(DateTime));
        RuleFor(x => x.expiresAt).GreaterThan(x => x.issuedAt);
        RuleFor(x => x.lastHeartbeatAt).NotEqual(default(DateTime));
    }
}