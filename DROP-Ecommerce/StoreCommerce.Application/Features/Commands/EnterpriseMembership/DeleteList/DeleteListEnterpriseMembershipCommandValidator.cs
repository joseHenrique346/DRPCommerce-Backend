using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEnterpriseMembershipCommandValidator : AbstractValidator<DeleteListEnterpriseMembershipCommand>
{
    public DeleteListEnterpriseMembershipCommandValidator()
    {
        RuleFor(command => command.ids).NotEmpty();
        RuleForEach(command => command.ids).GreaterThan(0);
    }
}
