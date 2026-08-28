using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateFraudSignalCommandValidator : AbstractValidator<UpdateFraudSignalCommand>
{
    private const string IpRegex = @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$";

    public UpdateFraudSignalCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.queueEntryId).GreaterThan(0);
        RuleFor(x => x.signalTypeId).GreaterThan(0);
        RuleFor(x => x.severityId).GreaterThan(0);
        RuleFor(x => x.description).NotEmpty();
        RuleFor(x => x.ipAddress).NotEmpty().Matches(IpRegex);
        RuleFor(x => x.deviceFingerprint).NotEmpty();
        RuleFor(x => x.detectedAt).NotEqual(default(DateTime));
    }
}