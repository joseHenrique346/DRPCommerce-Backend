using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropAuditLogCommandValidator : AbstractValidator<CreateListDropAuditLogCommand>
{
    public CreateListDropAuditLogCommandValidator()
    {
        RuleFor(x => x.commands).NotEmpty();
        RuleForEach(x => x.commands).SetValidator(new CreateDropAuditLogCommandValidator());
    }
}
