using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropAuditLogCommandValidator : AbstractValidator<CreateDropAuditLogCommand>
{
    private const string IpRegex = @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$";

    public CreateDropAuditLogCommandValidator()
    {
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0).When(x => x.customerId.HasValue);
        RuleFor(x => x.employeeId).GreaterThan(0).When(x => x.employeeId.HasValue);
        RuleFor(x => x.action).NotEmpty().MaximumLength(50);
        RuleFor(x => x.entityName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.entityId).GreaterThan(0);
        RuleFor(x => x.ipAddress).NotEmpty().Matches(IpRegex);
        RuleFor(x => x.userAgent).NotEmpty();
        RuleFor(x => x.ocurredAt).NotEqual(default(DateTime));
    }
}