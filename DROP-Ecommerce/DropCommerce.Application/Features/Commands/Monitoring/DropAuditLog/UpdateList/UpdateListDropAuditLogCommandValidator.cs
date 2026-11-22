using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropAuditLogCommandValidator : AbstractValidator<UpdateListDropAuditLogCommand>
{
    public UpdateListDropAuditLogCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new UpdateDropAuditLogCommandValidator());
    }
}
