using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropAuditLogCommandValidator : AbstractValidator<DeleteListDropAuditLogCommand>
{
    public DeleteListDropAuditLogCommandValidator()
    {
        RuleFor(x => x.ids).NotEmpty();
        RuleForEach(x => x.ids).GreaterThan(0);
    }
}
