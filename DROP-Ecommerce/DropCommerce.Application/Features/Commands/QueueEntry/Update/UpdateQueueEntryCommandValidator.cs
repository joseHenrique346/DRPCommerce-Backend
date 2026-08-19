using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateQueueEntryCommandValidator : AbstractValidator<UpdateQueueEntryCommand>
{
    private const string IpRegex = @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$";

    public UpdateQueueEntryCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.sessionToken).NotEmpty();
        RuleFor(x => x.position).GreaterThan(0);
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.deviceFingerprint).NotEmpty();
        RuleFor(x => x.ipAddress).NotEmpty().Matches(IpRegex);
        RuleFor(x => x.userAgent).NotEmpty();
        RuleFor(x => x.enteredAt).NotEqual(default(DateTime));
    }
}