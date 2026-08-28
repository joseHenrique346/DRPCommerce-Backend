using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropAuditLogCommandValidator : AbstractValidator<DeleteDropAuditLogCommand>
{
    public DeleteDropAuditLogCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
    }
}