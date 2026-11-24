using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEnterpriseMembershipCommandValidator : AbstractValidator<UpdateListEnterpriseMembershipCommand>
{
    public UpdateListEnterpriseMembershipCommandValidator()
    {
        RuleFor(command => command.commands).NotEmpty();
        RuleForEach(command => command.commands).SetValidator(new UpdateEnterpriseMembershipCommandValidator());
    }
}
